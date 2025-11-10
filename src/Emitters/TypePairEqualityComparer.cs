using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Emitters;

internal sealed class TypePairEqualityComparer : IEqualityComparer<(INamedTypeSymbol Source, INamedTypeSymbol Destination)>
{
    public static readonly TypePairEqualityComparer Instance = new();

    public bool Equals((INamedTypeSymbol Source, INamedTypeSymbol Destination) x, (INamedTypeSymbol Source, INamedTypeSymbol Destination) y)
    {
        return SymbolEqualityComparer.Default.Equals(x.Source, y.Source) && SymbolEqualityComparer.Default.Equals(x.Destination, y.Destination);
    }

    public int GetHashCode((INamedTypeSymbol Source, INamedTypeSymbol Destination) obj)
    {
        int hash = SymbolEqualityComparer.Default.GetHashCode(obj.Source);
        hash = unchecked((hash * 397) ^ SymbolEqualityComparer.Default.GetHashCode(obj.Destination));
        return hash;
    }
}

