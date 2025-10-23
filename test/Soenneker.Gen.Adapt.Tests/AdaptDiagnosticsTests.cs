using AwesomeAssertions;
using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Tests for Adapt method diagnostic reporting when methods cannot be created.
/// These tests should generate compiler ERRORS that prevent compilation.
/// The actual diagnostic errors will be visible in the IDE and prevent successful build.
/// </summary>
public sealed class AdaptDiagnosticsTests : UnitTest
{
    public AdaptDiagnosticsTests(ITestOutputHelper output) : base(output)
    {
    }

    //[Fact]
    //public void Adapt_NoParameterlessConstructor_ShouldGenerateDiagnostic()
    //{
    //    // This should trigger SGA002 diagnostic: "No parameterless constructor available"
    //    var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
    //    // This line should generate a compilation ERROR because NoParameterlessCtorDest
    //    // doesn't have a parameterless constructor. The diagnostic will prevent compilation.
    //    // Note: This test will fail to compile with error SGA002.
    //    var result = source.Adapt<NoParameterlessCtorDest>();
        
    //    // This code will never execute because compilation will fail with error SGA002
    //    result.Should().NotBeNull();
    //}

    //[Fact]
    //public void Adapt_Interface_ShouldGenerateDiagnostic()
    //{
    //    var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
    //    // This line should generate a compilation ERROR because IInterfaceDest
    //    // is an interface and cannot be instantiated. The diagnostic will prevent compilation.
    //    // Note: This test will fail to compile with error SGA002.
    //    var result = source.Adapt<IInterfaceDest>();
        
    //    // This code will never execute because compilation will fail with error SGA002
    //    result.Should().NotBeNull();
    //}

    //[Fact]
    //public void Adapt_NoMatchingProperties_ShouldGenerateDiagnostic()
    //{
    //    var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
    //    // This line should generate a compilation ERROR because NoMatchingPropsDest
    //    // has no properties that match the source properties. The diagnostic will prevent compilation.
    //    // Note: This test will fail to compile with error SGA003.
    //    var result = source.Adapt<NoMatchingPropsDest>();
        
    //    // This code will never execute because compilation will fail with error SGA003
    //    result.Should().NotBeNull();
    //}

    //[Fact]
    //public void Adapt_ReadOnlyProperties_ShouldGenerateDiagnostic()
    //{
    //    var source = new DiagnosticTestSource { Name = "test", Value = 42 };
        
    //    // This line should generate a compilation ERROR because ReadOnlyDest
    //    // has no settable properties. The diagnostic will prevent compilation.
    //    // Note: This test will fail to compile with error SGA003.
    //    var result = source.Adapt<ReadOnlyDest>();
        
    //    // This code will never execute because compilation will fail with error SGA003
    //    result.Should().NotBeNull();
    //}

    [Fact]
    public void Adapt_ValidMapping_ShouldNotGenerateDiagnostic()
    {
        // Use a valid source type that has matching properties with BasicDest
        var source = new BasicSource { Id = "1", Name = "test" };
        
        // This should work without any diagnostics since BasicDest has:
        // 1. A parameterless constructor
        // 2. Matching settable properties
        var result = source.Adapt<BasicDest>();
        
        // The result should be properly mapped
        result.Should().NotBeNull();
        result.Id.Should().Be("1");
        result.Name.Should().Be("test");
    }
}
