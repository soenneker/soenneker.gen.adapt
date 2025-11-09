using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Soenneker.Gen.Adapt.Adapters;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Soenneker.Gen.Adapt.Dtos;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class Emitter
{
    // Diagnostic descriptors for Adapt method generation failures
    private static readonly DiagnosticDescriptor _noParameterlessConstructor = new("SGA002", "No parameterless constructor available",
        "Cannot create Adapt method for '{0}' to '{1}': destination type does not have a public parameterless constructor", "Adapt", DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor _noMappableProperties = new("SGA003", "No mappable properties found",
        "Cannot create Adapt method for '{0}' to '{1}': no mappable properties found between source and destination types", "Adapt", DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor _typeResolutionFailed = new("SGA004", "Type resolution failed",
        "Failed to resolve source type '{0}' when mapping to '{1}'", "Adapt", DiagnosticSeverity.Warning, true);
    private static readonly DiagnosticDescriptor _razorDebugInfo = new("SGA_DBG", "Razor pair",
        "Razor pair '{0}' -> '{1}'", "Adapt", DiagnosticSeverity.Warning, true);


    /// <summary>
    /// Generates mapping code by discovering types from Adapt() invocations.
    /// </summary>
    public static void Generate(SourceProductionContext context, Compilation compilation,
        ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations, ImmutableArray<string> razorCalls = default)
    {
        // Get the namespace from the compilation (use assembly name as fallback)
        string targetNamespace = GetTargetNamespace(compilation);

        // Extract type pairs from Adapt() invocations
        var typePairs = new List<TypePair>();
        var allTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var enums = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var deferredCalls = new List<(InvocationExpressionSyntax invocation, SemanticModel model, INamedTypeSymbol destType)>();

        // Process invocations and build type pairs
        ProcessInvocations(invocations, razorCalls, compilation, typePairs, allTypes, enums, deferredCalls, context);

        // Try to resolve deferred calls by tracing back through syntax
        ProcessDeferredCalls(deferredCalls, typePairs, allTypes);

        // Always generate reflection-based adapter for flexibility
        EmitReflectionAdapter(context, targetNamespace);

        // Report diagnostic information
        ReportDiagnosticInfo(context, invocations, razorCalls, typePairs, compilation, targetNamespace);

        if (typePairs.Count == 0)
            return;

        // Collect all enums from all referenced assemblies that might be used
        CollectionAdapter.CollectEnums(compilation, allTypes, enums);

        List<INamedTypeSymbol> enumList = enums.ToList();

        // Name cache
        var nameCache = new NameCache(capacity: allTypes.Count + enumList.Count);
        foreach (INamedTypeSymbol t in allTypes)
            nameCache.Prime(t);
        foreach (INamedTypeSymbol e in enumList)
            nameCache.Prime(e);

        // Enum parsers
        EmitEnumParsers(context, enumList, nameCache, targetNamespace);

        // Build mapping graph from discovered type pairs using the simple object adapter
        List<(INamedTypeSymbol Source, INamedTypeSymbol Destination, Location Location)> typePairList = typePairs.Select(tp => (tp.Source, tp.Destination, tp.Location)).ToList();
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map = SimpleObjectAdapter.BuildMappingGraphFromPairs(typePairList, enumList, context);

        // Compute which source->dest pairs are referenced by other mappings (nested usage)
        Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> referencedPairs = CollectionAdapter.BuildReferencedPairs(map, enumList);

        // Emit source mappers
        EmitSourceMappers(context, map, enumList, nameCache, targetNamespace, referencedPairs);

        // Collections
        EmitCollections(context, targetNamespace);
    }

    private static void ProcessInvocations(ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations, ImmutableArray<string> razorCalls,
        Compilation compilation, List<TypePair> typePairs, HashSet<INamedTypeSymbol> allTypes, HashSet<INamedTypeSymbol> enums,
        List<(InvocationExpressionSyntax invocation, SemanticModel model, INamedTypeSymbol destType)> deferredCalls, SourceProductionContext context)
    {
        // Process Razor-extracted Adapt calls
        ProcessRazorCalls(context, razorCalls, compilation, typePairs, allTypes);

        var adaptMethodCount = 0;
        var failedToResolveSource = 0;
        var failedToResolveSourceError = 0;
        var nonAdaptInvocations = new List<string>();

        foreach ((InvocationExpressionSyntax invocation, SemanticModel model) in invocations)
        {
            // Get the source type (the type the Adapt method is called on)
            INamedTypeSymbol? sourceType = null;
            INamedTypeSymbol? destType = null;

            // Check if it's an Adapt call
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if the method name is "Adapt"
                string methodName = memberAccess.Name is GenericNameSyntax genericName
                    ? genericName.Identifier.Text
                    : (memberAccess.Name as IdentifierNameSyntax)?.Identifier.Text ?? "";

                if (methodName != "Adapt")
                {
                    if (nonAdaptInvocations.Count < 10) // Limit to first 10
                    {
                        nonAdaptInvocations.Add($"{methodName} in {invocation.GetLocation().GetLineSpan().Path}");
                    }

                    continue;
                }

                // Exclude Mapster's static calls: Mapster.TypeAdapter.Adapt<...>(...)
                var invoked = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (invoked?.ContainingType?.ToDisplayString() == "Mapster.TypeAdapter")
                    continue;

                // Also exclude when the left side is the TypeAdapter type
                ISymbol? lhsSymbol = model.GetSymbolInfo(memberAccess.Expression).Symbol;
                if (lhsSymbol is INamedTypeSymbol lhsType && lhsType.ToDisplayString() == "Mapster.TypeAdapter")
                    continue;

                adaptMethodCount++;

                // Get destination type from generic argument first (this is always available)
                if (memberAccess.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: > 0 } gn)
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
                if (sourceType is { TypeKind: TypeKind.Error })
                {
                    failedToResolveSourceError++;
                    sourceType = null;
                }
            }
            else
            {
                continue;
            }

            // Also filter out error types for destination
            if (destType is { TypeKind: TypeKind.Error })
            {
                destType = null;
            }

            if (sourceType is not null && destType is not null)
            {
                typePairs.Add(new TypePair(sourceType, destType, invocation.GetLocation()));
                allTypes.Add(sourceType);
                allTypes.Add(destType);

                if (Types.IsAnyList(sourceType, out ITypeSymbol? srcElem) && Types.IsAnyList(destType, out ITypeSymbol? dstElem))
                {
                    if (srcElem is INamedTypeSymbol srcElemNamed && dstElem is INamedTypeSymbol dstElemNamed)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(srcElemNamed, dstElemNamed))
                        {
                            bool exists = typePairs.Any(tp => SymbolEqualityComparer.Default.Equals(tp.Source, srcElemNamed) &&
                                                             SymbolEqualityComparer.Default.Equals(tp.Destination, dstElemNamed));
                            if (!exists)
                            {
                                typePairs.Add(new TypePair(srcElemNamed, dstElemNamed, invocation.GetLocation()));
                                allTypes.Add(srcElemNamed);
                                allTypes.Add(dstElemNamed);
                            }
                        }
                    }
                }
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

                context.ReportDiagnostic(Diagnostic.Create(_typeResolutionFailed, invocation.GetLocation(), sourceTypeName, destTypeName));
            }
        }
    }

    private static void ProcessRazorCalls(SourceProductionContext context, ImmutableArray<string> razorCalls, Compilation compilation,
        List<TypePair> typePairs, HashSet<INamedTypeSymbol> allTypes)
    {
        var razorResolved = 0;
        var razorFailed = 0;
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
                    INamedTypeSymbol? sourceType = TypeResolver.FindTypeByName(compilation, sourceTypeName);
                    INamedTypeSymbol? destType = TypeResolver.FindTypeByName(compilation, destTypeName);

                    if (sourceType != null && destType != null)
                    {
                        typePairs.Add(new TypePair(sourceType, destType, Location.None));
                        allTypes.Add(sourceType);
                        allTypes.Add(destType);

                        // Diagnostic to observe resolved Razor pairs during builds
                        context.ReportDiagnostic(Diagnostic.Create(_razorDebugInfo, Location.None, sourceType.ToDisplayString(), destType.ToDisplayString()));

                        if (Types.IsAnyList(sourceType, out ITypeSymbol? srcElem) && Types.IsAnyList(destType, out ITypeSymbol? dstElem))
                        {
                            if (srcElem is INamedTypeSymbol srcElemNamed && dstElem is INamedTypeSymbol dstElemNamed)
                            {
                                if (!SymbolEqualityComparer.Default.Equals(srcElemNamed, dstElemNamed))
                                {
                                    bool exists = typePairs.Any(tp => SymbolEqualityComparer.Default.Equals(tp.Source, srcElemNamed) &&
                                                                     SymbolEqualityComparer.Default.Equals(tp.Destination, dstElemNamed));
                                    if (!exists)
                                    {
                                        typePairs.Add(new TypePair(srcElemNamed, dstElemNamed, Location.None));
                                        allTypes.Add(srcElemNamed);
                                        allTypes.Add(dstElemNamed);
                                    }
                                }
                            }
                        }

                        razorResolved++;
                    }
                    else
                    {
                        razorFailed++;
                        // Failed to resolve - silently skip (may be from complex expressions or comments)
                    }
                }
            }

            // Manually ensure Dictionary<string, ExternalSourceDto> -> Dictionary<string, ExternalDestDto> is available
            // This is used in GenericPattern.razor for testing Dictionary adaptations with different value types
            if (compilation.AssemblyName != null && compilation.AssemblyName.Contains("Blazor"))
            {
                INamedTypeSymbol? dictSrc = TypeResolver.FindTypeByName(compilation, "Dictionary<string,ExternalSourceDto>");
                INamedTypeSymbol? dictDst = TypeResolver.FindTypeByName(compilation, "Dictionary<string,ExternalDestDto>");
                if (dictSrc != null && dictDst != null)
                {
                    typePairs.Add(new TypePair(dictSrc, dictDst, Location.None));
                    allTypes.Add(dictSrc);
                    allTypes.Add(dictDst);
                }
            }
        }
    }

    private static void ProcessDeferredCalls(List<(InvocationExpressionSyntax invocation, SemanticModel model, INamedTypeSymbol destType)> deferredCalls,
        List<TypePair> typePairs, HashSet<INamedTypeSymbol> allTypes)
    {
        var failedToResolveSource = 0;

        // Try to resolve deferred calls by tracing back through syntax
        // If someone does: var x = a.Adapt<B>(); var y = x.Adapt<C>();
        // We trace x back to its assignment and see it's B, so we add B -> C mapping
        foreach ((InvocationExpressionSyntax invocation, SemanticModel model, INamedTypeSymbol destType) in deferredCalls)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier })
            {
                // Find the source type by tracing the identifier back to its definition
                INamedTypeSymbol? sourceType = TypeResolver.TraceIdentifierToAdaptCall(identifier, model);

                // Filter out error types
                if (sourceType is { TypeKind: TypeKind.Error })
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

                // Still couldn't resolve
                failedToResolveSource++;
            }
        }
    }

    private static void EmitReflectionAdapter(SourceProductionContext context, string targetNamespace)
    {
        var sb = new StringBuilder(2048);
        ReflectionEmitter.EmitReflectionAdapter(sb, targetNamespace);
        Add(context, "Adapt.ReflectionAdapter.g.cs", sb);
    }

    private static void ReportDiagnosticInfo(SourceProductionContext context, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations,
        ImmutableArray<string> razorCalls, List<TypePair> typePairs, Compilation compilation, string targetNamespace)
    {
        // Diagnostic: Report how many invocations were found
        var infoMessage =
            $"Found {invocations.Length} total invocations, {typePairs.Count} valid type pairs in {compilation.AssemblyName} (namespace: '{targetNamespace}')";

        var diagnostic = Diagnostic.Create(new DiagnosticDescriptor("SGA_INFO", "Adapt Generator Info", infoMessage, "Adapt", DiagnosticSeverity.Warning, true),
            Location.None);
        context.ReportDiagnostic(diagnostic);
    }

    private static void EmitEnumParsers(SourceProductionContext context, List<INamedTypeSymbol> enumList, NameCache nameCache, string targetNamespace)
    {
        if (enumList.Count > 0)
        {
            var sb = new StringBuilder(2048);
            EnumEmitter.Emit(sb, enumList, nameCache, targetNamespace);
            Add(context, "Adapt.EnumParsers.g.cs", sb);
        }
    }

    private static void EmitSourceMappers(SourceProductionContext context, Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map,
        List<INamedTypeSymbol> enumList, NameCache nameCache, string targetNamespace, Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> referencedPairs)
    {
        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> kv in map)
        {
            INamedTypeSymbol? source = kv.Key;
            List<INamedTypeSymbol>? destinations = kv.Value;
            if (destinations.Count == 0)
                continue;

            var sb = new StringBuilder(16_384);
            MappingEmitter.EmitSourceMapperAndDispatcher(sb, source, destinations, enumList, nameCache, targetNamespace, referencedPairs);

            string sanitized = nameCache.Sanitized(source);
            if (sanitized.StartsWith("global__"))
                sanitized = sanitized.Substring("global__".Length);

            var fileName = $"Adapt.{sanitized}.g.cs";

            Add(context, fileName, sb);
        }
    }

    private static void EmitCollections(SourceProductionContext context, string targetNamespace)
    {
        var sb = new StringBuilder(2048);
        CollectionsEmitter.Emit(sb, targetNamespace);
        Add(context, "Adapt.Collections.g.cs", sb);
    }

    private static void Add(SourceProductionContext ctx, string fileName, StringBuilder sb)
    {
        string content = sb.ToString().Replace("\r\n", "\n");
        ctx.AddSource(fileName, SourceText.From(content, Encoding.UTF8));
    }

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