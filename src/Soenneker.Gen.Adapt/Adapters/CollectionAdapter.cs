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
                            AddReferencedPair(referenced, sValNamed, dValNamed);
                        }

                        if (Types.IsAnyList(sVal, out ITypeSymbol? sValElem) && Types.IsAnyList(dVal, out ITypeSymbol? dValElem) &&
                            Assignment.CanAssign(sValElem!, dValElem!, enums))
                        {
                            if (sVal is INamedTypeSymbol sValListNamed && dVal is INamedTypeSymbol dValListNamed)
                            {
                                AddReferencedPair(referenced, sValListNamed, dValListNamed);
                            }

                            if (sValElem is INamedTypeSymbol sElemNamed && dValElem is INamedTypeSymbol dElemNamed &&
                                !Types.IsFrameworkType(sElemNamed) && !Types.IsFrameworkType(dElemNamed))
                            {
                                AddReferencedPair(referenced, sElemNamed, dElemNamed);
                            }
                        }

                        if (Types.IsAnyDictionary(sVal, out ITypeSymbol? sInnerKey, out ITypeSymbol? sInnerVal) &&
                            Types.IsAnyDictionary(dVal, out ITypeSymbol? dInnerKey, out ITypeSymbol? dInnerVal) &&
                            SymbolEqualityComparer.Default.Equals(sInnerKey, dInnerKey) && Assignment.CanAssign(sInnerVal!, dInnerVal!, enums))
                        {
                            if (sVal is INamedTypeSymbol sValDictNamed && dVal is INamedTypeSymbol dValDictNamed)
                            {
                                AddReferencedPair(referenced, sValDictNamed, dValDictNamed);
                            }

                            if (sInnerVal is INamedTypeSymbol sInnerNamed && dInnerVal is INamedTypeSymbol dInnerNamed &&
                                !Types.IsFrameworkType(sInnerNamed) && !Types.IsFrameworkType(dInnerNamed))
                            {
                                AddReferencedPair(referenced, sInnerNamed, dInnerNamed);
                            }
                        }
                    }
                }
            }
        }

        return referenced;
    }

    private static void AddReferencedPair(Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> referenced, INamedTypeSymbol source, INamedTypeSymbol destination)
    {
        if (!referenced.TryGetValue(source, out HashSet<INamedTypeSymbol>? set))
        {
            set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            referenced[source] = set;
        }

        set.Add(destination);
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