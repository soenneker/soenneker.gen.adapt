namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Class with no matching properties - should trigger SGA003 diagnostic
public class NoMatchingPropsDest
{
    public string DifferentName { get; set; } = null!;
    public int DifferentValue { get; set; }
}


