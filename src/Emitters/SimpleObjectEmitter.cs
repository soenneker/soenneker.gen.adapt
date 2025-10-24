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
        string dstFq = names.ShortName(dest);

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
            // Handle collection mappings
            if (Types.IsArray(sp.Type, out ITypeSymbol? srcElement) && Types.IsArray(dp.Type, out ITypeSymbol? dstElement))
            {
                // Array to Array mapping
                sb.Append(indent).Append("if (source.").Append(sp.Name).Append(" != null)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\tint n = source.").Append(sp.Name).AppendLine(".Length;");
                sb.Append(indent).Append("\tvar targetArray = new ").Append(Types.ShortName(dp.Type).Replace("[]", "")).AppendLine("[n];");
                sb.AppendLine();
                sb.Append(indent).Append("\tfor (int i = 0; i < n; i++)").AppendLine();
                sb.Append(indent).AppendLine("\t{");
                if (SymbolEqualityComparer.Default.Equals(srcElement, dstElement))
                {
                    sb.Append(indent).Append("\t\ttargetArray[i] = source.").Append(sp.Name).AppendLine("[i];");
                }
                else
                {
                    string itemExpr = GetConversionExpression("source." + sp.Name + "[i]", srcElement, dstElement, names);
                    sb.Append(indent).Append("\t\ttargetArray[i] = ").Append(itemExpr).AppendLine(";");
                }
                sb.Append(indent).AppendLine("\t}");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = targetArray;").AppendLine();
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("else");
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = Array.Empty<").Append(Types.ShortName(dp.Type).Replace("[]", "")).AppendLine(">();");
                sb.Append(indent).AppendLine("}");
                continue;
            }
            else if (Types.IsArray(sp.Type, out ITypeSymbol? srcArrayElement) && Types.IsAnyList(dp.Type, out ITypeSymbol? dstListElement))
            {
                // Array to List mapping
                sb.Append(indent).Append("if (source.").Append(sp.Name).Append(" != null)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\tint n = source.").Append(sp.Name).AppendLine(".Length;");
                sb.AppendLine();
                sb.Append(indent).Append("\tvar targetList = new ").Append(Types.ShortName(dp.Type)).AppendLine("(n);");
                sb.AppendLine();
                if (SymbolEqualityComparer.Default.Equals(srcArrayElement, dstListElement))
                {
                    sb.Append(indent).Append("\tCollectionsMarshal.SetCount(targetList, n);").AppendLine();
                    sb.Append(indent).Append("\tsource.").Append(sp.Name).Append(".AsSpan().CopyTo(CollectionsMarshal.AsSpan(targetList));").AppendLine();
                }
                else
                {
                    sb.Append(indent).Append("\tfor (int i = 0; i < n; i++)").AppendLine();
                    sb.Append(indent).AppendLine("\t{");
                    string itemExpr = GetConversionExpression("source." + sp.Name + "[i]", srcArrayElement, dstListElement, names);
                    sb.Append(indent).Append("\t\ttargetList.Add(").Append(itemExpr).AppendLine(");");
                    sb.Append(indent).AppendLine("\t}");
                }
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = targetList;").AppendLine();
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("else");
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = new ").Append(Types.ShortName(dp.Type)).AppendLine("();");
                sb.Append(indent).AppendLine("}");
                continue;
            }
            else if (Types.IsAnyList(sp.Type, out ITypeSymbol? srcListElement) && Types.IsArray(dp.Type, out ITypeSymbol? dstArrayElement))
            {
                // List to Array mapping
                sb.Append(indent).Append("if (source.").Append(sp.Name).Append(" != null)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\tint n = source.").Append(sp.Name).AppendLine(".Count;");
                sb.Append(indent).Append("\tvar targetArray = new ").Append(Types.ShortName(dp.Type).Replace("[]", "")).AppendLine("[n];");
                sb.AppendLine();
                sb.Append(indent).Append("\tfor (int i = 0; i < n; i++)").AppendLine();
                sb.Append(indent).AppendLine("\t{");
                if (SymbolEqualityComparer.Default.Equals(srcListElement, dstArrayElement))
                {
                    sb.Append(indent).Append("\t\ttargetArray[i] = source.").Append(sp.Name).AppendLine("[i];");
                }
                else
                {
                    string itemExpr = GetConversionExpression("source." + sp.Name + "[i]", srcListElement, dstArrayElement, names);
                    sb.Append(indent).Append("\t\ttargetArray[i] = ").Append(itemExpr).AppendLine(";");
                }
                sb.Append(indent).AppendLine("\t}");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = targetArray;").AppendLine();
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("else");
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = Array.Empty<").Append(Types.ShortName(dp.Type).Replace("[]", "")).AppendLine(">();");
                sb.Append(indent).AppendLine("}");
                continue;
            }
            else if (Types.IsAnyList(sp.Type, out ITypeSymbol? srcListToElement) && Types.IsAnyList(dp.Type, out ITypeSymbol? dstListToElement))
            {
                sb.Append(indent).Append("if (source.").Append(sp.Name).Append(" != null)").AppendLine();
                sb.Append(indent).AppendLine("{");
                
                // Generate proper collection mapping logic
                if (Types.IsList(sp.Type, out _))
                {
                    sb.Append(indent).Append("\tvar src = CollectionsMarshal.AsSpan(source.").Append(sp.Name).AppendLine(");");
                    sb.Append(indent).Append("\tint n = src.Length;").AppendLine();
                    sb.AppendLine();
                    sb.Append(indent).Append("\tvar targetList = new ").Append(Types.ShortName(dp.Type)).AppendLine("(n);");
                    sb.AppendLine();
                    if (SymbolEqualityComparer.Default.Equals(srcListToElement, dstListToElement))
                    {
                        sb.Append(indent).Append("\tCollectionsMarshal.SetCount(targetList, n);").AppendLine();
                        sb.Append(indent).Append("\tsrc.CopyTo(CollectionsMarshal.AsSpan(targetList));").AppendLine();
                    }
                    else
                    {
                        sb.Append(indent).Append("\tfor (int i = 0; i < n; i++)").AppendLine();
                        sb.Append(indent).AppendLine("\t{");
                        sb.Append(indent).Append("\t\tref readonly var s = ref src[i];").AppendLine();
                        string itemExpr = GetConversionExpression("s", srcListToElement, dstListToElement, names);
                        sb.Append(indent).Append("\t\ttargetList.Add(").Append(itemExpr).AppendLine(");");
                        sb.Append(indent).AppendLine("\t}");
                    }
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = targetList;").AppendLine();
                }
                else
                {
                    // Handle IEnumerable<T> and other collection types
                    sb.Append(indent).Append("\tvar targetList = new ").Append(Types.ShortName(dp.Type)).AppendLine("();");
                    sb.Append(indent).Append("\tforeach (var item in source.").Append(sp.Name).AppendLine(")");
                    sb.Append(indent).AppendLine("\t{");
                    if (SymbolEqualityComparer.Default.Equals(srcListToElement, dstListToElement))
                    {
                        sb.Append(indent).Append("\t\ttargetList.Add(item);").AppendLine();
                    }
                    else
                    {
                        string itemExpr = GetConversionExpression("item", srcListToElement, dstListToElement, names);
                        sb.Append(indent).Append("\t\ttargetList.Add(").Append(itemExpr).AppendLine(");");
                    }
                    sb.Append(indent).AppendLine("\t}");
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = targetList;").AppendLine();
                }
                
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("else");
                sb.Append(indent).AppendLine("{");
                // Initialize as empty collection when source is null
                if (Types.IsArray(dp.Type, out _))
                {
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = Array.Empty<").Append(Types.ShortName(dp.Type).Replace("[]", "")).AppendLine(">();");
                }
                else
                {
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = new ").Append(Types.ShortName(dp.Type)).AppendLine("();");
                }
                sb.Append(indent).AppendLine("}");
                continue;
            }

            // Handle dictionary mappings
            if (Types.IsAnyDictionary(sp.Type, out ITypeSymbol? srcKey, out ITypeSymbol? srcValue) &&
                Types.IsAnyDictionary(dp.Type, out ITypeSymbol? dstKey, out ITypeSymbol? dstValue))
            {
                sb.Append(indent).Append("if (source.").Append(sp.Name).Append(" != null)").AppendLine();
                sb.Append(indent).AppendLine("{");
                
                // Generate proper dictionary mapping logic
                sb.Append(indent).Append("\tvar targetDict = new ").Append(Types.ShortName(dp.Type)).AppendLine("();");
                sb.Append(indent).Append("\tforeach (var kvp in source.").Append(sp.Name).AppendLine(")");
                sb.Append(indent).AppendLine("\t{");
                
                string keyExpr = SymbolEqualityComparer.Default.Equals(srcKey, dstKey) ? "kvp.Key" : GetConversionExpression("kvp.Key", srcKey, dstKey, names);
                string valueExpr = SymbolEqualityComparer.Default.Equals(srcValue, dstValue) ? "kvp.Value" : GetConversionExpression("kvp.Value", srcValue, dstValue, names);
                
                sb.Append(indent).Append("\t\ttargetDict[").Append(keyExpr).Append("] = ").Append(valueExpr).AppendLine(";");
                sb.Append(indent).AppendLine("\t}");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = targetDict;").AppendLine();
                
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("else");
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = new ").Append(Types.ShortName(dp.Type)).AppendLine("();");
                sb.Append(indent).AppendLine("}");
                continue;
            }

            // Handle nested object mappings
            if (sp.Type is INamedTypeSymbol srcNamed && dp.Type is INamedTypeSymbol dstNamed &&
                (srcNamed.TypeKind == TypeKind.Class || srcNamed.TypeKind == TypeKind.Struct) &&
                (dstNamed.TypeKind == TypeKind.Class || dstNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(srcNamed) &&
                !Types.IsFrameworkType(dstNamed) && !SymbolEqualityComparer.Default.Equals(srcNamed, dstNamed))
            {
                string srcSanLocal = names.Sanitized(srcNamed);
                string dstSanLocal = names.Sanitized(dstNamed);
                
                // Only generate null checks for reference types (classes), not value types (structs)
                if (srcNamed.TypeKind == TypeKind.Class)
                {
                    sb.Append(indent).Append("if (source.").Append(sp.Name).AppendLine(" is not null)");
                    sb.Append(indent).AppendLine("{");
                    sb.Append(indent).Append("\ttarget.").Append(dp.Name).Append(" = Map_").Append(srcSanLocal).Append("_To_").Append(dstSanLocal).Append("(")
                        .Append("source.").Append(sp.Name).AppendLine(");");
                    sb.Append(indent).AppendLine("}");
                }
                else // Struct
                {
                    sb.Append(indent).Append("target.").Append(dp.Name).Append(" = Map_").Append(srcSanLocal).Append("_To_").Append(dstSanLocal).Append("(")
                        .Append("source.").Append(sp.Name).AppendLine(");");
                }

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

    private static string GetConversionExpression(string expr, ITypeSymbol fromType, ITypeSymbol toType, NameCache names)
    {
        // Handle basic type conversions
        if (SymbolEqualityComparer.Default.Equals(fromType, toType))
            return expr;

        // Handle enum conversions
        if (fromType.TypeKind == TypeKind.Enum && Types.IsString(toType))
            return expr + ".ToString()";

        if (Types.IsString(fromType) && toType.TypeKind == TypeKind.Enum)
            return "GenAdapt_EnumParsers.Parse_" + San((INamedTypeSymbol)toType) + "(" + expr + ")";

        if (fromType.TypeKind == TypeKind.Enum && Types.IsInt(toType))
            return "(int)" + expr;

        if (Types.IsInt(fromType) && toType.TypeKind == TypeKind.Enum)
            return "(" + Types.Fq(toType) + ")" + expr;

        // Handle Guid conversions
        if (Types.IsGuid(fromType) && Types.IsString(toType))
            return expr + ".ToString()";

        if (Types.IsString(fromType) && Types.IsGuid(toType))
            return "global::System.Guid.TryParse(" + expr + ", out var g) ? g : default(global::System.Guid)";

        // Handle nullable conversions
        if (Types.IsNullableOf(fromType, out ITypeSymbol? fromInner) && Types.IsNullableOf(toType, out ITypeSymbol? toInner))
        {
            if (SymbolEqualityComparer.Default.Equals(fromInner!, toInner!))
                return expr; // same nullable type
        }

        // Handle user-defined type conversions
        if (fromType is INamedTypeSymbol fromNamed && toType is INamedTypeSymbol toNamed &&
            (fromNamed.TypeKind == TypeKind.Class || fromNamed.TypeKind == TypeKind.Struct) &&
            (toNamed.TypeKind == TypeKind.Class || toNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(fromNamed) && !Types.IsFrameworkType(toNamed))
        {
            string fromSan = names.Sanitized(fromNamed);
            string toSan = names.Sanitized(toNamed);
            if (fromNamed.TypeKind == TypeKind.Struct)
                return "Map_" + fromSan + "_To_" + toSan + "(in " + expr + ")";
            return "Map_" + fromSan + "_To_" + toSan + "(" + expr + ")";
        }

        // Handle interface to concrete type conversions
        if (fromType.TypeKind == TypeKind.Interface && toType is INamedTypeSymbol toNamedType && 
            (toNamedType.TypeKind == TypeKind.Class || toNamedType.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(toNamedType))
        {
            // For interface to concrete type, we need to use the Adapt method
            return "(" + expr + ").Adapt<" + Types.ShortName(toNamedType) + ">()";
        }

        // Default: cast
        return "(" + Types.Fq(toType) + ")" + expr;
    }

    private static string San(INamedTypeSymbol type)
    {
        // Simple sanitization for enum parser names
        return type.Name.Replace(".", "_").Replace("`", "_");
    }
}