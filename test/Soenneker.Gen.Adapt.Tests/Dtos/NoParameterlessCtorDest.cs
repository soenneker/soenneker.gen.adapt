namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Class without parameterless constructor - should trigger SGA002 diagnostic
public class NoParameterlessCtorDest
{
    public string Name { get; set; } = null!;

    public NoParameterlessCtorDest(string name)
    {
        Name = name;
    }
}

