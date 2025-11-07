using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Soenneker.Gen.Adapt.Dtos;

namespace Soenneker.Gen.Adapt.Adapters;

internal static class SimpleObjectAdapter
{
    /// <summary>
    /// Handles simple object-to-object adaptations (non-collection, non-dictionary types)
    /// </summary>
    public static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> BuildMappingGraphFromPairs(
        List<(INamedTypeSymbol Source, INamedTypeSymbol Destination, Location Location)> typePairs, List<INamedTypeSymbol> enums,
        SourceProductionContext context)
    {
        var map = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(SymbolEqualityComparer.Default);

        foreach ((INamedTypeSymbol src, INamedTypeSymbol dst, Location location) in typePairs)
        {
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
                        AddNestedPairs(map, src, dst, enums);
                    }

                    continue;
                }
            }

            // Special handling for Dictionary-to-Dictionary adaptations
            if (Types.IsAnyDictionary(src, out ITypeSymbol? srcKey, out ITypeSymbol? srcValue) &&
                Types.IsAnyDictionary(dst, out ITypeSymbol? dstKey, out ITypeSymbol? dstValue))
            {
                // Allow Dictionary<K1,V1> to Dictionary<K2,V2> if keys are same and values are compatible
                bool keysMatch = SymbolEqualityComparer.Default.Equals(srcKey, dstKey);
                bool valuesCompatible = Assignment.CanAssign(srcValue!, dstValue!, enums);

                if (keysMatch && valuesCompatible)
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
                        AddNestedPairs(map, src, dst, enums);
                    }

                    continue;
                }
                else
                {
                    // Diagnostic: Dictionary adaptation failed compatibility check
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("SGA_DICT_SKIP", "Dictionary Adaptation Skipped",
                            $"Skipping Dictionary adaptation '{src.ToDisplayString()}' -> '{dst.ToDisplayString()}': keysMatch={keysMatch}, valuesCompatible={valuesCompatible}, srcKey={srcKey?.ToDisplayString()}, dstKey={dstKey?.ToDisplayString()}, srcValue={srcValue?.ToDisplayString()}, dstValue={dstValue?.ToDisplayString()}",
                            "Adapt", DiagnosticSeverity.Info, true), location));
                    continue;
                }
            }

            // Special handling for Array-to-Array adaptations
            if (Types.IsArray(src, out ITypeSymbol? srcArrayElement) && Types.IsArray(dst, out ITypeSymbol? dstArrayElement))
            {
                // Allow Array<T1> to Array<T2> if element types are compatible
                if (Assignment.CanAssign(srcArrayElement, dstArrayElement, enums))
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
                        AddNestedPairs(map, src, dst, enums);
                    }

                    continue;
                }
            }

            // Special handling for Array-to-Collection adaptations
            if (Types.IsArray(src, out ITypeSymbol? srcArrayToCollectionElement) && Types.IsAnyList(dst, out ITypeSymbol? dstListFromArrayElement))
            {
                // Allow Array<T1> to List<T2> if element types are compatible
                if (Assignment.CanAssign(srcArrayToCollectionElement, dstListFromArrayElement, enums))
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
                        AddNestedPairs(map, src, dst, enums);
                    }

                    continue;
                }
            }

            // Special handling for Collection-to-Array adaptations
            if (Types.IsAnyList(src, out ITypeSymbol? srcListToArrayElement) && Types.IsArray(dst, out ITypeSymbol? dstArrayFromListElement))
            {
                // Allow List<T1> to Array<T2> if element types are compatible
                if (Assignment.CanAssign(srcListToArrayElement, dstArrayFromListElement, enums))
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
                        AddNestedPairs(map, src, dst, enums);
                    }

                    continue;
                }
            }

            // Validate that mapping is possible for simple objects
            if (!HasParameterlessCtor(dst))
            {
                // Report diagnostic for missing parameterless constructor
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGA002", "No parameterless constructor available",
                        "Cannot create Adapt method for '{0}' to '{1}': destination type does not have a public parameterless constructor", "Adapt",
                        DiagnosticSeverity.Error, true), location, src.ToDisplayString(), dst.ToDisplayString()));
                continue;
            }

            var srcProps = TypeProps.Build(src);
            var dstProps = TypeProps.Build(dst);

            if (!HasAnyMappableProperty(src, dst, srcProps, dstProps, enums))
            {
                // Report diagnostic for no mappable properties
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGA003", "No mappable properties found",
                        "Cannot create Adapt method for '{0}' to '{1}': no mappable properties found between source and destination types", "Adapt",
                        DiagnosticSeverity.Error, true), location, src.ToDisplayString(), dst.ToDisplayString()));
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
                AddNestedPairs(map, src, dst, enums);
            }
        }

        return map;
    }

    private static bool HasParameterlessCtor(INamedTypeSymbol type)
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
    private static bool HasAnyMappableProperty(INamedTypeSymbol srcType, INamedTypeSymbol dstType, TypeProps src, TypeProps dst,
        List<INamedTypeSymbol> enums)
    {
        if (Types.IsAnyList(srcType, out _) && Types.IsAnyList(dstType, out _))
            return true;

        if (Types.IsAnyDictionary(srcType, out ITypeSymbol? srcKey, out ITypeSymbol? srcVal) &&
            Types.IsAnyDictionary(dstType, out ITypeSymbol? dstKey, out ITypeSymbol? dstVal) &&
            SymbolEqualityComparer.Default.Equals(srcKey, dstKey) && Assignment.CanAssign(srcVal!, dstVal!, enums))
            return true;

        if (Types.IsArray(srcType, out _) && Types.IsArray(dstType, out _))
            return true;

        if (Types.IsArray(srcType, out _) && Types.IsAnyList(dstType, out _))
            return true;

        if (Types.IsAnyList(srcType, out _) && Types.IsArray(dstType, out _))
            return true;

        for (var i = 0; i < dst.Settable.Count; i++)
        {
            Prop d = dst.Settable[i];
            if (!src.TryGet(d.Name, out Prop s))
                continue;

            if (Types.IsAnyList(s.Type, out _) && Types.IsAnyList(d.Type, out _))
                return true;

            if (Types.IsAnyDictionary(s.Type, out ITypeSymbol? sKey, out _) && Types.IsAnyDictionary(d.Type, out ITypeSymbol? dKey, out _) &&
                SymbolEqualityComparer.Default.Equals(sKey, dKey))
                return true;

            // Check for array-to-array mappings
            if (Types.IsArray(s.Type, out _) && Types.IsArray(d.Type, out _))
                return true;

            // Check for array-to-collection mappings
            if (Types.IsArray(s.Type, out _) && Types.IsAnyList(d.Type, out _))
                return true;

            // Check for collection-to-array mappings
            if (Types.IsAnyList(s.Type, out _) && Types.IsArray(d.Type, out _))
                return true;

            if (Assignment.CanAssign(s.Type, d.Type, enums))
                return true;
        }

        return false;
    }

    private static void AddNestedPairs(Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map, INamedTypeSymbol src, INamedTypeSymbol dst,
        List<INamedTypeSymbol> enums)
    {
        // Walk properties to ensure nested user-defined type pairs are present
        var srcProps = TypeProps.Build(src);
        var dstProps = TypeProps.Build(dst);

        for (var i = 0; i < dstProps.Settable.Count; i++)
        {
            Prop dp = dstProps.Settable[i];
            if (!srcProps.TryGet(dp.Name, out Prop sp))
                continue;

            // Nested object
            if (sp.Type is INamedTypeSymbol sNamed && dp.Type is INamedTypeSymbol dNamed &&
                (sNamed.TypeKind == TypeKind.Class || sNamed.TypeKind == TypeKind.Struct) &&
                (dNamed.TypeKind == TypeKind.Class || dNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(sNamed) && !Types.IsFrameworkType(dNamed) &&
                !SymbolEqualityComparer.Default.Equals(sNamed, dNamed))
            {
                if (!map.TryGetValue(sNamed, out List<INamedTypeSymbol>? list))
                {
                    list = new List<INamedTypeSymbol>(4);
                    map[sNamed] = list;
                }

                if (!list.Contains(dNamed, SymbolEqualityComparer.Default))
                {
                    list.Add(dNamed);
                }
            }

            // Nested collection element types
            if (Types.IsAnyList(sp.Type, out ITypeSymbol? sElem) && Types.IsAnyList(dp.Type, out ITypeSymbol? dElem))
            {
                if (sElem is INamedTypeSymbol sElemNamed && dElem is INamedTypeSymbol dElemNamed && !Types.IsFrameworkType(sElemNamed) &&
                    !Types.IsFrameworkType(dElemNamed))
                {
                    if (!map.TryGetValue(sElemNamed, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>(4);
                        map[sElemNamed] = list;
                    }

                    if (!list.Contains(dElemNamed, SymbolEqualityComparer.Default))
                    {
                        list.Add(dElemNamed);
                    }
                }
            }

            if (Types.IsAnyDictionary(sp.Type, out ITypeSymbol? sKey, out ITypeSymbol? sVal) &&
                Types.IsAnyDictionary(dp.Type, out ITypeSymbol? dKey, out ITypeSymbol? dVal))
            {
                if (sVal is INamedTypeSymbol sValNamed && dVal is INamedTypeSymbol dValNamed && SymbolEqualityComparer.Default.Equals(sKey, dKey) &&
                    Assignment.CanAssign(sVal, dVal, enums) && !Types.IsFrameworkType(sValNamed) && !Types.IsFrameworkType(dValNamed))
                {
                    if (!map.TryGetValue(sValNamed, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>(4);
                        map[sValNamed] = list;
                    }

                    if (!list.Contains(dValNamed, SymbolEqualityComparer.Default))
                    {
                        list.Add(dValNamed);
                    }
                }
            }
        }
    }
}