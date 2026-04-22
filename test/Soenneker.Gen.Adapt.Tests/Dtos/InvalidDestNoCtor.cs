namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Invalid destination - no parameterless constructor (should generate SGA002)
public class InvalidDestNoCtor
{
    public string Name { get; set; } = string.Empty;

    public InvalidDestNoCtor(string name)
    {
        Name = name;
    }
}

