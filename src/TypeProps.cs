using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt;



internal sealed class TypeProps
{
    private readonly Dictionary<string, Prop> _readable; // by name
    public readonly List<Prop> Settable;

    private TypeProps(Dictionary<string, Prop> readable, List<Prop> settable)
    {
        _readable = readable;
        Settable = settable;
    }

    public static TypeProps Build(INamedTypeSymbol type)
    {
        var readable = new Dictionary<string, Prop>(32);
        var settable = new List<Prop>(32);

        foreach (ISymbol? m in type.GetMembers())
        {
            if (m is not IPropertySymbol p) continue;
            if (p.DeclaredAccessibility != Accessibility.Public) continue;

            if (p.GetMethod is not null)
                readable[p.Name] = new Prop(p.Name, p.Type);

            // Treat init-only as settable too
            bool hasSet = p.SetMethod is not null;
            bool isInit = p.SetMethod?.IsInitOnly ?? false;
            if (hasSet || isInit)
                settable.Add(new Prop(p.Name, p.Type));
        }

        return new TypeProps(readable, settable);
    }

    public bool TryGet(string name, out Prop p) => _readable.TryGetValue(name, out p);
}
