using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Types;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class ExternalProjectTests : UnitTest
{
    public ExternalProjectTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_FromExternalProjectDto_ToBasicDest_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ExternalProjectDto
        {
            Id = "external-123"
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("external-123");
    }

    [Fact]
    public void Adapt_FromBasicSource_ToExternalProjectDto_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource
        {
            Id = "internal-456",
            Name = "Test",
            Count = 10
        };

        // Act
        var result = source.Adapt<ExternalProjectDto>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("internal-456");
    }

    [Fact]
    public void Adapt_ExternalProjectDto_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = new ExternalProjectDto
        {
            Id = "roundtrip-789"
        };

        // Act
        var intermediate = original.Adapt<BasicDest>();
        var result = intermediate.Adapt<ExternalProjectDto>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("roundtrip-789");
    }

    [Fact]
    public void Adapt_ExternalProjectDto_WithNullId_ShouldMapNull()
    {
        // Arrange
        var source = new ExternalProjectDto
        {
            Id = null
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeNull();
    }

    [Fact]
    public void Adapt_ExternalProjectDto_WithEmptyId_ShouldMapEmpty()
    {
        // Arrange
        var source = new ExternalProjectDto
        {
            Id = ""
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("");
    }
}

