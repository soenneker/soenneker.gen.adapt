namespace Soenneker.Gen.Adapt.Tests.Dtos;

// Class without parameterless constructor - should trigger SGA002 diagnostic
public class NoParameterlessCtorDest
{
    public string Name { get; set; } = string.Empty;
    
    public NoParameterlessCtorDest(string name)
    {
        Name = name;
    }
}

// Class with only getter properties - should trigger SGA003 diagnostic (no settable properties)
public class ReadOnlyDest
{
    public string Name { get; }
    public int Value { get; }
}

// Interface - should trigger SGA002 diagnostic (no parameterless constructor)
public interface IInterfaceDest
{
    string Name { get; set; }
}

// Class with no matching properties - should trigger SGA003 diagnostic
public class NoMatchingPropsDest
{
    public string DifferentName { get; set; } = string.Empty;
    public int DifferentValue { get; set; }
}

// Source class for testing
public class DiagnosticTestSource
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
