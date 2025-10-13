using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Soenneker.Gen.Adapt;

internal static class Emitter
{
    // Diagnostic descriptors for Adapt method generation failures
    private static readonly DiagnosticDescriptor NoParameterlessConstructor = new(
        "SGA002", 
        "No parameterless constructor available", 
        "Cannot create Adapt method for '{0}' to '{1}': destination type does not have a public parameterless constructor", 
        "Adapt", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor NoMappableProperties = new(
        "SGA003", 
        "No mappable properties found", 
        "Cannot create Adapt method for '{0}' to '{1}': no mappable properties found between source and destination types", 
        "Adapt", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor TypeResolutionFailed = new(
        "SGA004", 
        "Type resolution failed", 
        "Cannot create Adapt method: failed to resolve source type '{0}' or destination type '{1}'", 
        "Adapt", 
        DiagnosticSeverity.Error, 
        true);

    private readonly struct TypePair
    {
        public readonly INamedTypeSymbol Source;
        public readonly INamedTypeSymbol Destination;
        public readonly Location Location;

        public TypePair(INamedTypeSymbol source, INamedTypeSymbol destination, Location location)
        {
            Source = source;
            Destination = destination;
            Location = location;
        }
    }

    /// <summary>
    /// Generates mapping code by discovering types from Adapt() invocations.
    /// </summary>
    public static void Generate(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations)
    {
        // Extract type pairs from Adapt() invocations
        var typePairs = new List<TypePair>();
        var allTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var enums = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach ((InvocationExpressionSyntax invocation, SemanticModel model) in invocations)
        {
            // Get the source type (the type the Adapt method is called on)
            INamedTypeSymbol? sourceType = null;
            INamedTypeSymbol? destType = null;

            // Check if it's an extension method call: source.Adapt<TDest>()
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if the method name is "Adapt"
                string methodName = memberAccess.Name is GenericNameSyntax genericName
                    ? genericName.Identifier.Text
                    : (memberAccess.Name as IdentifierNameSyntax)?.Identifier.Text ?? "";

                if (methodName != "Adapt")
                    continue;

                // Try GetTypeInfo first, fallback to SymbolInfo if needed
                TypeInfo typeInfo = model.GetTypeInfo(memberAccess.Expression);
                ITypeSymbol? expressionType = typeInfo.Type ?? typeInfo.ConvertedType;
                
                // If still null, try to get from symbol info
                if (expressionType is null)
                {
                    SymbolInfo symbolInfo = model.GetSymbolInfo(memberAccess.Expression);
                    
                    if (symbolInfo.Symbol is ILocalSymbol local)
                    {
                        expressionType = local.Type;
                    }
                    else if (symbolInfo.Symbol is IParameterSymbol parameter)
                    {
                        expressionType = parameter.Type;
                    }
                    else if (symbolInfo.Symbol is IPropertySymbol property)
                    {
                        expressionType = property.Type;
                    }
                    else if (symbolInfo.Symbol is IFieldSymbol field)
                    {
                        expressionType = field.Type;
                    }
                }
                
                sourceType = expressionType as INamedTypeSymbol;

                // Get destination type from generic argument
                if (memberAccess.Name is GenericNameSyntax gn && gn.TypeArgumentList.Arguments.Count > 0)
                {
                    TypeSyntax destTypeSyntax = gn.TypeArgumentList.Arguments[0];
                    ITypeSymbol? destTypeSymbol = model.GetTypeInfo(destTypeSyntax).Type;
                    destType = destTypeSymbol as INamedTypeSymbol;
                }
            }
            else
            {
                continue;
            }

            if (sourceType is not null && destType is not null)
            {
                typePairs.Add(new TypePair(sourceType, destType, invocation.GetLocation()));
                allTypes.Add(sourceType);
                allTypes.Add(destType);
            }
            else
            {
                // Report diagnostic for type resolution failures
                string sourceTypeName = sourceType?.ToDisplayString() ?? "unknown";
                string destTypeName = destType?.ToDisplayString() ?? "unknown";
                
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeResolutionFailed,
                    invocation.GetLocation(),
                    sourceTypeName, destTypeName));
            }
        }

        // Always generate reflection-based adapter for flexibility
        // This handles scenarios where compile-time type info isn't available (generic type parameters, etc.)
        {
            var sb = new StringBuilder(2048);
            ReflectionAdapter.EmitReflectionAdapter(sb);
            Add(context, "Adapt.ReflectionAdapter.g.cs", sb);
        }

        if (typePairs.Count == 0)
            return;

        // Collect all enums from all referenced assemblies that might be used
        CollectEnums(compilation, allTypes, enums);

        var enumList = enums.ToList();

        // Name cache
        var nameCache = new NameCache(capacity: allTypes.Count + enumList.Count);
        foreach (INamedTypeSymbol t in allTypes)
            nameCache.Prime(t);
        foreach (INamedTypeSymbol e in enumList)
            nameCache.Prime(e);

        // Enum parsers
        if (enumList.Count > 0)
        {
            var sb = new StringBuilder(2048);
            EnumParsers.Emit(sb, enumList, nameCache);
            Add(context, "Adapt.EnumParsers.g.cs", sb);
        }

        // Build mapping graph from discovered type pairs
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map = BuildMappingGraphFromPairs(typePairs, enumList, context);

        // Per-source mapper files
        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> kv in map)
        {
            INamedTypeSymbol? source = kv.Key;
            List<INamedTypeSymbol>? destinations = kv.Value;
            if (destinations.Count == 0)
                continue;

            var sb = new StringBuilder(16_384);
            MapperFile.EmitSourceMapperAndDispatcher(sb, source, destinations, enumList, nameCache);

            string sanitized = nameCache.Sanitized(source);
            if (sanitized.StartsWith("global__"))
                sanitized = sanitized.Substring("global__".Length);

            var fileName = $"Adapt.{sanitized}.g.cs";

            Add(context, fileName, sb);
        }

        // Collections
        {
            var sb = new StringBuilder(2048);
            Collections.Emit(sb);
            Add(context, "Adapt.Collections.g.cs", sb);
        }
    }

    private static void CollectEnums(Compilation compilation, HashSet<INamedTypeSymbol> types, HashSet<INamedTypeSymbol> enums)
    {
        // Scan types and their properties to find enums
        foreach (INamedTypeSymbol type in types)
        {
            // Get all properties
            foreach (ISymbol member in type.GetMembers())
            {
                if (member is IPropertySymbol prop)
                {
                    ITypeSymbol propType = prop.Type;
                    
                    // Check if the property type is an enum
                    if (propType is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Enum)
                    {
                        enums.Add(namedType);
                    }
                    
                    // Check for nullable enum
                    if (Types.IsNullableOf(propType, out ITypeSymbol? inner) && inner is INamedTypeSymbol innerNamed && innerNamed.TypeKind == TypeKind.Enum)
                    {
                        enums.Add(innerNamed);
                    }
                }
            }
        }
    }

    private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> BuildMappingGraphFromPairs(
        List<TypePair> typePairs,
        List<INamedTypeSymbol> enums,
        SourceProductionContext context)
    {
        var map = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);

        foreach (TypePair pair in typePairs)
        {
            INamedTypeSymbol src = pair.Source;
            INamedTypeSymbol dst = pair.Destination;
            Location location = pair.Location;

            if (!HasParameterlessCtorLocal(dst))
            {
                // Report diagnostic for missing parameterless constructor
                context.ReportDiagnostic(Diagnostic.Create(
                    NoParameterlessConstructor,
                    location,
                    src.ToDisplayString(), dst.ToDisplayString()));
                continue;
            }

            // Validate that mapping is possible
            TypeProps srcProps = TypeProps.Build(src);
            TypeProps dstProps = TypeProps.Build(dst);

            if (!HasAnyMappableProperty(srcProps, dstProps, enums))
            {
                // Report diagnostic for no mappable properties
                context.ReportDiagnostic(Diagnostic.Create(
                    NoMappableProperties,
                    location,
                    src.ToDisplayString(), dst.ToDisplayString()));
                continue;
            }

            if (!map.TryGetValue(src, out List<INamedTypeSymbol>? list))
            {
                list = new List<INamedTypeSymbol>(8);
                map[src] = list;
            }

            if (!list.Contains(dst, SymbolEqualityComparer.Default))
            {
                list.Add(dst);
            }
        }

        return map;
    }


    private static bool HasParameterlessCtorLocal(INamedTypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Struct)
            return true;

        if (type.TypeKind == TypeKind.Interface)
            return false;

        foreach (IMethodSymbol? ctor in type.InstanceConstructors)
            if (ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public)
                return true;

        return false;
    }

    /// <summary>
    /// Checks mapping feasibility using same rules as emission.
    /// </summary>
    private static bool HasAnyMappableProperty(TypeProps src, TypeProps dst, List<INamedTypeSymbol> enums)
    {
        for (int i = 0; i < dst.Settable.Count; i++)
        {
            Prop d = dst.Settable[i];
            if (!src.TryGet(d.Name, out Prop s))
                continue;

            if (Types.IsAnyList(s.Type, out _) && Types.IsAnyList(d.Type, out _))
                return true;

            if (Types.IsAnyDictionary(s.Type, out ITypeSymbol? sKey, out _) && 
                Types.IsAnyDictionary(d.Type, out ITypeSymbol? dKey, out _) &&
                SymbolEqualityComparer.Default.Equals(sKey, dKey))
                return true;

            if (Assignment.CanAssign(s.Type, d.Type, enums))
                return true;
        }
        return false;
    }

    private static void Add(SourceProductionContext ctx, string fileName, StringBuilder sb)
        => ctx.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));
}
