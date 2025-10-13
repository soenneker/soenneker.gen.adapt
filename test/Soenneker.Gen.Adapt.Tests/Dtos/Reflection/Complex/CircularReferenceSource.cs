namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class CircularReferenceSource
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Note: Circular references would need special handling in real scenarios
    // For reflection, we just copy assignable properties
}

