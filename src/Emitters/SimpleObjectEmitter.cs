using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Soenneker.Gen.Adapt.Dtos;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class SimpleObjectEmitter
{
    /// <summary>
    /// Handles mapping body generation for simple object-to-object mappings
    /// </summary>
    public static void EmitMappingBodyInstructions(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, List<INamedTypeSymbol> enums,
        NameCache names, string indent)
    {
        var srcProps = TypeProps.Build(source);
        var dstProps = TypeProps.Build(dest);
        string dstFq = names.FullyQualified(dest);

        // Find required properties and init-only properties
        HashSet<string> requiredPropertyNames = GetRequiredPropertyNames(dest);
        HashSet<string> initOnlyPropertyNames = GetInitOnlyPropertyNames(dest);
        bool hasRequiredProps = requiredPropertyNames.Count > 0;
        bool hasInitOnlyProps = initOnlyPropertyNames.Count > 0;

        // Build list of property mappings
        var simpleMappings = new List<(string propName, string value)>();
        var complexMappings = new List<(Prop dp, Prop sp)>();

        for (var i = 0; i < dstProps.Settable.Count; i++)
        {
            Prop dp = dstProps.Settable[i];
            if (!srcProps.TryGet(dp.Name, out Prop sp))
            {
                // No source property - if required, add default
                if (requiredPropertyNames.Contains(dp.Name))
                {
                    string defaultValue = GetDefaultValueForType(dp.Type);
                    simpleMappings.Add((dp.Name, defaultValue));
                }

                continue;
            }

            // Check if it's a complex type that needs special handling
            bool isComplexType = Types.IsAnyList(sp.Type, out _) || Types.IsAnyDictionary(sp.Type, out _, out _);

            if (isComplexType)
            {
                complexMappings.Add((dp, sp));
                continue;
            }

            // Simple property assignment
            string? rhs = Assignment.TryBuild("source." + sp.Name, sp.Type, dp.Type, enums);
            if (rhs is null)
            {
                // Check if it's a nested object that needs mapping
                if (sp.Type is INamedTypeSymbol srcNamed && dp.Type is INamedTypeSymbol dstNamed &&
                    (srcNamed.TypeKind == TypeKind.Class || srcNamed.TypeKind == TypeKind.Struct) &&
                    (dstNamed.TypeKind == TypeKind.Class || dstNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(srcNamed) &&
                    !Types.IsFrameworkType(dstNamed) && !SymbolEqualityComparer.Default.Equals(srcNamed, dstNamed))
                {
                    // This is a nested object that needs mapping
                    complexMappings.Add((dp, sp));
                }
                else if (Types.IsString(sp.Type) && Types.IsGuid(dp.Type))
                {
                    // Special case - will handle after object creation
                    complexMappings.Add((dp, sp));
                }

                continue;
            }

            simpleMappings.Add((dp.Name, rhs));
        }

        // Emit object creation
        if (hasRequiredProps || hasInitOnlyProps)
        {
            // Use object initializer syntax for required or init-only properties
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine();
            sb.Append(indent).AppendLine("{");
            for (var i = 0; i < simpleMappings.Count; i++)
            {
                (string propName, string value) = simpleMappings[i];
                sb.Append(indent).Append("\t").Append(propName).Append(" = ").Append(value);
                if (i < simpleMappings.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }

            sb.Append(indent).AppendLine("};");
        }
        else
        {
            // Traditional approach
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("();");

            foreach ((string propName, string value) in simpleMappings)
            {
                sb.Append(indent).Append("target.").Append(propName).Append(" = ").Append(value).AppendLine(";");
            }
        }

        // Handle complex mappings (lists, dictionaries, special cases)
        foreach ((Prop dp, Prop sp) in complexMappings)
        {
            // Handle nested object mappings
            if (sp.Type is INamedTypeSymbol srcNamed && dp.Type is INamedTypeSymbol dstNamed &&
                (srcNamed.TypeKind == TypeKind.Class || srcNamed.TypeKind == TypeKind.Struct) &&
                (dstNamed.TypeKind == TypeKind.Class || dstNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(srcNamed) &&
                !Types.IsFrameworkType(dstNamed) && !SymbolEqualityComparer.Default.Equals(srcNamed, dstNamed))
            {
                sb.Append(indent).Append("if (source.").Append(sp.Name).AppendLine(" is not null)");
                sb.Append(indent).AppendLine("{");
                string srcSanLocal = names.Sanitized(srcNamed);
                string dstSanLocal = names.Sanitized(dstNamed);
                if (srcNamed.TypeKind == TypeKind.Struct)
                {
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = Map_").Append(srcSanLocal).Append("_To_").Append(dstSanLocal).Append("(")
                        .Append("in source.").Append(sp.Name).AppendLine(");");
                }
                else
                {
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = Map_").Append(srcSanLocal).Append("_To_").Append(dstSanLocal).Append("(")
                        .Append("source.").Append(sp.Name).AppendLine(");");
                }

                sb.Append(indent).AppendLine("}");
                continue;
            }

            // Special case: string -> Guid with TryParse
            if (Types.IsString(sp.Type) && Types.IsGuid(dp.Type))
            {
                sb.Append(indent).Append("if (global::System.Guid.TryParse(source.").Append(sp.Name).AppendLine(", out var g))");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).AppendLine(" = g;");
            }
        }

        sb.Append(indent).AppendLine("return target;");
    }

    private static HashSet<string> GetRequiredPropertyNames(INamedTypeSymbol type)
    {
        var result = new HashSet<string>();
        foreach (ISymbol member in type.GetMembers())
        {
            if (member is IPropertySymbol { IsRequired: true } prop)
            {
                result.Add(prop.Name);
            }
        }

        return result;
    }

    private static HashSet<string> GetInitOnlyPropertyNames(INamedTypeSymbol type)
    {
        var result = new HashSet<string>();

        // Check all properties including base class properties
        INamedTypeSymbol? current = type;
        while (current is not null)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is IPropertySymbol { SetMethod.IsInitOnly: true } prop)
                {
                    result.Add(prop.Name);
                }
            }

            current = current.BaseType;
        }

        return result;
    }

    private static string GetDefaultValueForType(ITypeSymbol type)
    {
        if (Types.IsString(type))
            return "string.Empty";

        if (type.IsValueType)
        {
            if (Types.IsNullableOf(type, out _))
                return "null";
            return "default";
        }

        return "null!";
    }
}