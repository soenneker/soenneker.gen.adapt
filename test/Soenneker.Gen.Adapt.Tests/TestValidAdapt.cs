using Soenneker.Documents.Audit;
using Soenneker.Dtos.IdNamePair;
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
    
}