using System;
using AwesomeAssertions;
using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Tests to verify that valid Adapt methods are still generated even when some invalid ones
/// cause diagnostic errors. This demonstrates that the generator continues processing other
/// valid mappings after encountering failures.
/// </summary>
public sealed class MixedDiagnosticTests : UnitTest
{
    public MixedDiagnosticTests(ITestOutputHelper output) : base(output)
    {
    }

    //[Fact]
    public void ValidAdapt_ShouldWork_EvenWithDiagnosticErrors()
    {
        // This test demonstrates that valid Adapt methods are still generated
        // even when there are diagnostic errors for invalid mappings in the same project
        
        var source = new ValidSource { Name = "test", Value = 42 };
        
        // This should work fine - ValidDestination has parameterless constructor and matching properties
        var result = source.Adapt<ValidDestination>();
        
        result.Should().NotBeNull();
        result.Name.Should().Be("test");
        result.Value.Should().Be(42);
    }

    //[Fact]
    public void InvalidAdapt_ShouldGenerateDiagnosticError()
    {
        // This test will fail to compile with SGA002 diagnostic error
        // because InvalidDestNoCtor doesn't have a parameterless constructor
        
        var source = new ValidSource { Name = "test", Value = 42 };
        
        // This line should generate a compilation error SGA002
        var result = source.Adapt<InvalidDestNoCtor>();
        
        // This code will never execute because compilation will fail
        result.Should().NotBeNull();
    }

    //[Fact]
    public void AnotherInvalidAdapt_ShouldGenerateDiagnosticError()
    {
        // This test will fail to compile with SGA003 diagnostic error
        // because InvalidDestNoProps has no matching properties
        
        var source = new ValidSource { Name = "test", Value = 42 };
        
        // This line should generate a compilation error SGA003
        var result = source.Adapt<InvalidDestNoProps>();
        
        // This code will never execute because compilation will fail
        result.Should().NotBeNull();
    }
}
