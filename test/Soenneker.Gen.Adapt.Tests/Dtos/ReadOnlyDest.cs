namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Class with only getter properties - should trigger SGA003 diagnostic (no settable properties)
public class ReadOnlyDest
{
    public string Name { get; }
    public int Value { get; }
}

