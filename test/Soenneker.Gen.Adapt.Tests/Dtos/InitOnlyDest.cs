namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class InitOnlyDest
{
    public string Id { get; init; }
    public string Name { get; init; }
    public int Count { get; set; }
}

