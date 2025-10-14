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
    private static readonly DiagnosticDescriptor _noParameterlessConstructor = new(
        "SGA002", 
        "No parameterless constructor available", 
        "Cannot create Adapt method for '{0}' to '{1}': destination type does not have a public parameterless constructor", 
        "Adapt", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor _noMappableProperties = new(
        "SGA003", 
        "No mappable properties found", 
        "Cannot create Adapt method for '{0}' to '{1}': no mappable properties found between source and destination types", 
        "Adapt", 
        DiagnosticSeverity.Error, 
        true);

    private static readonly DiagnosticDescriptor _typeResolutionFailed = new(
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
        ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations,
        ImmutableArray<string> razorCalls = default)
    {
        // Get the namespace from the compilation (use assembly name as fallback)
        string targetNamespace = GetTargetNamespace(compilation);
        
        // Extract type pairs from Adapt() invocations
        var typePairs = new List<TypePair>();
        var allTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var enums = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var deferredCalls = new List<(InvocationExpressionSyntax invocation, SemanticModel model, INamedTypeSymbol destType)>();

        // Process Razor-extracted Adapt calls
        if (!razorCalls.IsDefaultOrEmpty)
        {
            foreach (string razorCall in razorCalls)
            {
                string[] parts = razorCall.Split('|');
                if (parts.Length == 2)
                {
                    string sourceTypeName = parts[0];
                    string destTypeName = parts[1];
                    
                    // Try to resolve types from compilation
                    INamedTypeSymbol? sourceType = FindTypeByName(compilation, sourceTypeName);
                    INamedTypeSymbol? destType = FindTypeByName(compilation, destTypeName);
                    
                    if (sourceType != null && destType != null)
                    {
                        typePairs.Add(new TypePair(sourceType, destType, Location.None));
                        allTypes.Add(sourceType);
                        allTypes.Add(destType);
                    }
                }
            }
        }

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

                // Get destination type from generic argument first (this is always available)
                if (memberAccess.Name is GenericNameSyntax gn && gn.TypeArgumentList.Arguments.Count > 0)
                {
                    TypeSyntax destTypeSyntax = gn.TypeArgumentList.Arguments[0];
                    ITypeSymbol? destTypeSymbol = model.GetTypeInfo(destTypeSyntax).Type;
                    destType = destTypeSymbol as INamedTypeSymbol;
                }

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
                
                // Filter out error types - these appear when the compiler can't resolve a type
                if (sourceType != null && sourceType.TypeKind == TypeKind.Error)
                {
                    sourceType = null;
                }
            }
            else
            {
                continue;
            }
            
            // Also filter out error types for destination
            if (destType != null && destType.TypeKind == TypeKind.Error)
            {
                destType = null;
            }

            if (sourceType is not null && destType is not null)
            {
                typePairs.Add(new TypePair(sourceType, destType, invocation.GetLocation()));
                allTypes.Add(sourceType);
                allTypes.Add(destType);
            }
            else if (sourceType is null && destType is not null)
            {
                // Defer this call - we might be able to resolve it after generating initial mappings
                // This handles cases like: var x = a.Adapt<B>(); var y = x.Adapt<C>();
                // where x's type depends on a.Adapt<B>() being generated first
                deferredCalls.Add((invocation, model, destType));
                allTypes.Add(destType);
            }
            else
            {
                // Report diagnostic for type resolution failures only if we also can't get dest type
                string sourceTypeName = sourceType?.ToDisplayString() ?? "unknown";
                string destTypeName = destType?.ToDisplayString() ?? "unknown";
                
                context.ReportDiagnostic(Diagnostic.Create(
                    _typeResolutionFailed,
                    invocation.GetLocation(),
                    sourceTypeName, destTypeName));
            }
        }
        
        // Try to resolve deferred calls by tracing back through syntax
        // If someone does: var x = a.Adapt<B>(); var y = x.Adapt<C>();
        // We trace x back to its assignment and see it's B, so we add B -> C mapping
        foreach ((InvocationExpressionSyntax invocation, SemanticModel model, INamedTypeSymbol destType) in deferredCalls)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                // Find the source type by tracing the identifier back to its definition
                INamedTypeSymbol? sourceType = TraceIdentifierToAdaptCall(identifier, model);
                
                // Filter out error types
                if (sourceType != null && sourceType.TypeKind == TypeKind.Error)
                {
                    sourceType = null;
                }
                
                if (sourceType is not null)
                {
                    typePairs.Add(new TypePair(sourceType, destType, invocation.GetLocation()));
                    allTypes.Add(sourceType);
                    // Successfully resolved - don't report any diagnostic
                    continue;
                }
                
                // Still couldn't resolve - but don't report diagnostic
                // The mapping might already exist from another invocation where types were known
                // If the mapping truly doesn't exist, compilation will fail with CS1061 anyway
            }
        }

        // Always generate reflection-based adapter for flexibility
        // This handles scenarios where compile-time type info isn't available (generic type parameters, etc.)
        {
            var sb = new StringBuilder(2048);
            ReflectionAdapter.EmitReflectionAdapter(sb, targetNamespace);
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
            EnumParsers.Emit(sb, enumList, nameCache, targetNamespace);
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
            MapperFile.EmitSourceMapperAndDispatcher(sb, source, destinations, enumList, nameCache, targetNamespace);

            string sanitized = nameCache.Sanitized(source);
            if (sanitized.StartsWith("global__"))
                sanitized = sanitized.Substring("global__".Length);

            var fileName = $"Adapt.{sanitized}.g.cs";

            Add(context, fileName, sb);
        }

        // Collections
        {
            var sb = new StringBuilder(2048);
            Collections.Emit(sb, targetNamespace);
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
                    _noParameterlessConstructor,
                    location,
                    src.ToDisplayString(), dst.ToDisplayString()));
                continue;
            }


            // Special handling for collection-to-collection adaptations
            if (Types.IsIEnumerable(src, out ITypeSymbol? srcElement) && Types.IsAnyList(dst, out ITypeSymbol? dstElement))
            {
                // Allow IEnumerable<T> to List<T> adaptations if element types are compatible
                if (Assignment.CanAssign(srcElement, dstElement, enums))
                {
                    // Add to the map for later processing
                    if (!map.TryGetValue(src, out List<INamedTypeSymbol>? destList))
                    {
                        destList = new List<INamedTypeSymbol>(8);
                        map[src] = destList;
                    }
                    // Only add if not already present to avoid duplicates
                    if (!destList.Contains(dst))
                    {
                        destList.Add(dst);
                    }
                    continue;
                }
            }

            // Validate that mapping is possible
            TypeProps srcProps = TypeProps.Build(src);
            TypeProps dstProps = TypeProps.Build(dst);

            if (!HasAnyMappableProperty(srcProps, dstProps, enums))
            {
                // Report diagnostic for no mappable properties
                context.ReportDiagnostic(Diagnostic.Create(
                    _noMappableProperties,
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

    /// <summary>
    /// Traces an identifier back to its source by looking at variable declarations and finding Adapt calls.
    /// For example, if we see "doc.Adapt&lt;Foo&gt;()" and doc is declared as "var doc = entity.Adapt&lt;Bar&gt;()",
    /// this will return Bar as the type.
    /// </summary>
    private static INamedTypeSymbol? TraceIdentifierToAdaptCall(IdentifierNameSyntax identifier, SemanticModel model)
    {
        // Try to get the symbol for this identifier
        var symbolInfo = model.GetSymbolInfo(identifier);
        ILocalSymbol? localSymbol = symbolInfo.Symbol as ILocalSymbol;
        
        if (localSymbol != null)
        {
            // Find the variable declarator for this local symbol
            var syntaxReference = localSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference != null)
            {
                var declaratorSyntax = syntaxReference.GetSyntax();
                
                // Check if it's a VariableDeclaratorSyntax with an initializer
                if (declaratorSyntax is VariableDeclaratorSyntax declarator && declarator.Initializer?.Value != null)
                {
                    // Check if the initializer is an Adapt call
                    if (declarator.Initializer.Value is InvocationExpressionSyntax invocation &&
                        invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is GenericNameSyntax genericName &&
                        genericName.Identifier.Text == "Adapt" &&
                        genericName.TypeArgumentList.Arguments.Count > 0)
                    {
                        // Get the generic type argument - this is what the variable's type will be
                        TypeSyntax destTypeSyntax = genericName.TypeArgumentList.Arguments[0];
                        ITypeSymbol? destTypeSymbol = model.GetTypeInfo(destTypeSyntax).Type;
                        return destTypeSymbol as INamedTypeSymbol;
                    }
                }
            }
        }
        else
        {
            // Symbol couldn't be resolved (e.g. because type depends on our generator)
            // Fall back to syntax-only search in the containing method/block
            var containingMethod = identifier.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod != null)
            {
                var identifierName = identifier.Identifier.Text;
                
                // Find variable declarations in the method before this usage
                var declarators = containingMethod.DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>()
                    .Where(v => v.Identifier.Text == identifierName);
                
                foreach (var declarator in declarators)
                {
                    // Check if this declarator comes before our identifier in the source
                    if (declarator.SpanStart < identifier.SpanStart && declarator.Initializer?.Value != null)
                    {
                        // Check if the initializer is an Adapt call
                        if (declarator.Initializer.Value is InvocationExpressionSyntax invocation &&
                            invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Name is GenericNameSyntax genericName &&
                            genericName.Identifier.Text == "Adapt" &&
                            genericName.TypeArgumentList.Arguments.Count > 0)
                        {
                            // Get the generic type argument - this is what the variable's type will be
                            TypeSyntax destTypeSyntax = genericName.TypeArgumentList.Arguments[0];
                            ITypeSymbol? destTypeSymbol = model.GetTypeInfo(destTypeSyntax).Type;
                            return destTypeSymbol as INamedTypeSymbol;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static INamedTypeSymbol? FindTypeByName(Compilation compilation, string typeName)
    {
        // Try to find the type in the compilation
        // Handle both simple names and fully qualified names
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName(typeName);
        if (type != null)
            return type;
        
        // If not found, try searching through all types in all assemblies
        var allTypes = compilation.GlobalNamespace.GetNamespaceMembers()
            .SelectMany(ns => GetAllTypes(ns))
            .Concat(GetAllTypes(compilation.GlobalNamespace));
        
        foreach (INamedTypeSymbol t in allTypes)
        {
            if (t.Name == typeName || t.ToDisplayString() == typeName)
                return t;
        }
        
        return null;
    }
    
    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
        foreach (INamedTypeSymbol type in ns.GetTypeMembers())
        {
            yield return type;
            foreach (INamedTypeSymbol nested in GetNestedTypes(type))
                yield return nested;
        }
        
        foreach (INamespaceSymbol childNs in ns.GetNamespaceMembers())
        {
            foreach (INamedTypeSymbol type in GetAllTypes(childNs))
                yield return type;
        }
    }
    
    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (INamedTypeSymbol deepNested in GetNestedTypes(nested))
                yield return deepNested;
        }
    }

    private static void Add(SourceProductionContext ctx, string fileName, StringBuilder sb)
        => ctx.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));

    private static string GetTargetNamespace(Compilation compilation)
    {
        // Use the assembly name as the namespace
        // Dots are valid in namespaces, so we keep them
        string assemblyName = compilation.AssemblyName ?? "GeneratedAdapt";
        
        // Only clean up characters that are invalid in namespaces
        assemblyName = assemblyName.Replace(" ", "").Replace("-", "_");
        
        return assemblyName;
    }
}
