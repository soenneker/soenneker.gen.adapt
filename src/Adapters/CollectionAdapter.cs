using Microsoft.CodeAnalysis;
using Soenneker.Gen.Adapt.Dtos;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Adapters;

internal static class CollectionAdapter
{
    /// <summary>
    /// Handles collection-to-collection adaptations and builds referenced pairs for nested collections
    /// </summary>
    public static Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> BuildReferencedPairs(Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map,
        List<INamedTypeSymbol> enums)
    {
        var referenced = new Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>(SymbolEqualityComparer.Default);

        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> kv in map)
        {
            INamedTypeSymbol src = kv.Key;
            List<INamedTypeSymbol> dests = kv.Value;

            foreach (INamedTypeSymbol dst in dests)
            {
                // Walk the dst's settable properties; if any property requires a user-defined mapping
                // from some nested source type to nested dest type, mark that nested mapping as referenced.
                var srcProps = TypeProps.Build(src);
                var dstProps = TypeProps.Build(dst);

                for (var i = 0; i < dstProps.Settable.Count; i++)
                {
                    Prop dp = dstProps.Settable[i];
                    if (!srcProps.TryGet(dp.Name, out Prop sp))
                        continue;

                    // Nested object mapping
                    if (sp.Type is INamedTypeSymbol sNamed && dp.Type is INamedTypeSymbol dNamed &&
                        (sNamed.TypeKind == TypeKind.Class || sNamed.TypeKind == TypeKind.Struct) &&
                        (dNamed.TypeKind == TypeKind.Class || dNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(sNamed) &&
                        !Types.IsFrameworkType(dNamed) && !SymbolEqualityComparer.Default.Equals(sNamed, dNamed))
                    {
                        if (!referenced.TryGetValue(sNamed, out HashSet<INamedTypeSymbol>? set))
                        {
                            set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                            referenced[sNamed] = set;
                        }

                        set.Add(dNamed);
                    }

                    // Nested lists
                    if (Types.IsAnyList(sp.Type, out ITypeSymbol? sElem) && Types.IsAnyList(dp.Type, out ITypeSymbol? dElem))
                    {
                        if (sElem is INamedTypeSymbol sElemNamed && dElem is INamedTypeSymbol dElemNamed && !Types.IsFrameworkType(sElemNamed) &&
                            !Types.IsFrameworkType(dElemNamed))
                        {
                            if (!referenced.TryGetValue(sElemNamed, out HashSet<INamedTypeSymbol>? set))
                            {
                                set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                                referenced[sElemNamed] = set;
                            }

                            set.Add(dElemNamed);
                        }
                    }

                    // Nested dictionaries (value types)
                    if (Types.IsAnyDictionary(sp.Type, out _, out ITypeSymbol? sVal) && Types.IsAnyDictionary(dp.Type, out _, out ITypeSymbol? dVal))
                    {
                        if (sVal is INamedTypeSymbol sValNamed && dVal is INamedTypeSymbol dValNamed && Assignment.CanAssign(sVal, dVal, enums) &&
                            !Types.IsFrameworkType(sValNamed) && !Types.IsFrameworkType(dValNamed))
                        {
                            if (!referenced.TryGetValue(sValNamed, out HashSet<INamedTypeSymbol>? set))
                            {
                                set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                                referenced[sValNamed] = set;
                            }

                            set.Add(dValNamed);
                        }
                    }
                }
            }
        }

        return referenced;
    }

    /// <summary>
    /// Collects enums from types and their properties
    /// </summary>
    public static void CollectEnums(Compilation compilation, HashSet<INamedTypeSymbol> types, HashSet<INamedTypeSymbol> enums)
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
                    if (propType is INamedTypeSymbol { TypeKind: TypeKind.Enum } namedType)
                    {
                        enums.Add(namedType);
                    }

                    // Check for nullable enum
                    if (Types.IsNullableOf(propType, out ITypeSymbol? inner) && inner is INamedTypeSymbol { TypeKind: TypeKind.Enum } innerNamed)
                    {
                        enums.Add(innerNamed);
                    }
                }
            }
        }
    }
}