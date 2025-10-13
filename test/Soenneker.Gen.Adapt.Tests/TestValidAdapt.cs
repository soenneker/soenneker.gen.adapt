using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Simple test to verify that valid Adapt methods are still generated
/// even when there are diagnostic errors for invalid mappings
/// </summary>
public class TestValidAdapt
{
    public void TestValidMapping()
    {
        var source = new ValidSource { Name = "test", Value = 42 };
        
        // This should work fine - ValidDestination has parameterless constructor and matching properties
        var result = source.Adapt<ValidDestination>();
        
        // The result should be properly mapped
        System.Console.WriteLine($"Name: {result.Name}, Value: {result.Value}");
    }
    
    public void TestInvalidMapping()
    {
        var source = new ValidSource { Name = "test", Value = 42 };
        
        // This will cause compilation error SGA002 - no parameterless constructor
        // var result1 = source.Adapt<InvalidDestNoCtor>();
        
        // This will cause compilation error SGA003 - no matching properties  
        // var result2 = source.Adapt<InvalidDestNoProps>();
    }
}
