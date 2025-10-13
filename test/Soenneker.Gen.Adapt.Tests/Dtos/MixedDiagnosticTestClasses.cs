using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Valid source class with matching properties
public class ValidSource
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

// Valid destination with parameterless constructor and matching properties
public class ValidDestination
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

// Invalid destination - no parameterless constructor (should generate SGA002)
public class InvalidDestNoCtor
{
    public string Name { get; set; } = string.Empty;
    
    public InvalidDestNoCtor(string name)
    {
        Name = name;
    }
}

// Invalid destination - no matching properties (should generate SGA003)
public class InvalidDestNoProps
{
    public string DifferentName { get; set; } = string.Empty;
    public int DifferentValue { get; set; }
}
