namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Invalid destination - no matching properties (should generate SGA003)
public class InvalidDestNoProps
{
    public string DifferentName { get; set; } = string.Empty;
    public int DifferentValue { get; set; }
}


