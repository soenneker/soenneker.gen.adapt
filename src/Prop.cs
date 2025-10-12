using Microsoft.CodeAnalysis;

namespace Soenneker.Gen.Adapt
{
    internal readonly struct Prop
    {
        public readonly string Name;
        public readonly ITypeSymbol Type;
        public Prop(string name, ITypeSymbol type) { Name = name; Type = type; }
    }
}
