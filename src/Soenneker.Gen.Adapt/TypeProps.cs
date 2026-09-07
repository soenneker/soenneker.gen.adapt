using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Soenneker.Gen.Adapt.Dtos;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt;

internal sealed class TypeProps
{
    private static readonly ConditionalWeakTable<INamedTypeSymbol, TypeProps> _cache = new();

    private readonly Dictionary<string, Prop> _readable; // by name
    public readonly List<Prop> Settable;

    private TypeProps(Dictionary<string, Prop> readable, List<Prop> settable)
    {
        _readable = readable;
        Settable = settable;
    }

    public static TypeProps Build(INamedTypeSymbol type) => _cache.GetValue(type, static symbol => BuildCore(symbol));

    private static TypeProps BuildCore(INamedTypeSymbol type)
    {
        var readable = new Dictionary<string, Prop>(32);
        var settable = new List<Prop>(32);
        var settableNames = new HashSet<string>();

        // Walk up the inheritance chain to include inherited properties
        INamedTypeSymbol? currentType = type;
        while (currentType is not null)
        {
            foreach (ISymbol? m in currentType.GetMembers())
            {
                if (m is not IPropertySymbol p)
                    continue;
                if (p.DeclaredAccessibility != Accessibility.Public)
                    continue;

                if (p.IsIndexer)
                    continue;

                if (p.GetMethod is not null && !readable.ContainsKey(p.Name))
                    readable[p.Name] = new Prop(p.Name, p.Type);

                // Treat init-only as settable too
                bool hasSet = p.SetMethod is not null;
                bool isInit = p.SetMethod?.IsInitOnly ?? false;
                if ((hasSet || isInit) && settableNames.Add(p.Name))
                {
                    settable.Add(new Prop(p.Name, p.Type));
                }
            }

            currentType = currentType.BaseType;
        }

        return new TypeProps(readable, settable);
    }

    public bool TryGet(string name, out Prop p) => _readable.TryGetValue(name, out p);
}