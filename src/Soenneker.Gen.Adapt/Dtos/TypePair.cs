using Microsoft.CodeAnalysis;

namespace Soenneker.Gen.Adapt.Dtos;

internal readonly struct TypePair
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