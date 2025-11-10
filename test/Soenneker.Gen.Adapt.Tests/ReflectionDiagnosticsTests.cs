using System;
using AwesomeAssertions;
using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Soenneker.Gen.Adapt.Tests.Dtos.Abstract;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Tests for reflection adapter diagnostics when types cannot be instantiated.
/// These tests verify that the reflection adapter properly handles edge cases
/// and provides meaningful error messages.
/// </summary>
public sealed class ReflectionDiagnosticsTests : UnitTest
{
    public ReflectionDiagnosticsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_NoParameterlessConstructor_ShouldThrow()
    {
        // This should trigger SGA002 diagnostic: "No parameterless constructor available"
        var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
        // The reflection fallback should fail for types without parameterless constructors
        Action act = () => source.AdaptViaReflection<NoParameterlessCtorDest>();
        
        // The reflection fallback should fail for types without parameterless constructors
        act.Should().Throw<MissingMethodException>()
            .WithMessage("*Cannot dynamically create an instance*");
    }

    [Fact]
    public void AdaptViaReflection_Interface_ShouldThrow()
    {
        var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
        // The reflection fallback should fail for interfaces
        Action act = () => source.AdaptViaReflection<IInterfaceDest>();
        
        // The reflection fallback should fail for interfaces
        act.Should().Throw<MissingMethodException>()
            .WithMessage("*Cannot dynamically create an instance*");
    }

    [Fact]
    public void AdaptViaReflection_NoMatchingProperties_ShouldSucceed()
    {
        var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
        // This should work with reflection fallback even though no properties match
        // The reflection adapter will create an instance with default values
        var result = source.AdaptViaReflection<NoMatchingPropsDest>();
        
        // The result should be created but with default values since no properties match
        result.Should().NotBeNull();
        result.DifferentName.Should().BeNullOrEmpty(); // Default string value
        result.DifferentValue.Should().Be(0); // Default int value
    }

    [Fact]
    public void AdaptViaReflection_ReadOnlyProperties_ShouldSucceed()
    {
        var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
        // This should work with reflection fallback even though properties are read-only
        // The reflection adapter will create an instance with default values
        var result = source.AdaptViaReflection<ReadOnlyDest>();
        
        // The result should be created but with default values since properties are read-only
        result.Should().NotBeNull();
        result.Name.Should().BeNullOrEmpty(); // Default string value
        result.Value.Should().Be(0); // Default int value
    }
}
