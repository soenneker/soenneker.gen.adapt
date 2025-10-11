using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class StructTests : UnitTest
{
    public StructTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_Struct_ShouldMapProperties()
    {
        // Arrange
        var source = new PointStructSource
        {
            X = 10,
            Y = 20
        };

        // Act
        var result = source.Adapt<PointStructDest>();

        // Assert
        result.X.Should().Be(10);
        result.Y.Should().Be(20);
    }

    [Fact]
    public void Adapt_ClassWithStructProperty_ShouldMapRecursively()
    {
        // Arrange
        var source = new MixedStructSource
        {
            Location = new PointStructSource { X = 100, Y = 200 },
            Label = "Point A"
        };

        // Act
        var result = source.Adapt<MixedStructDest>();

        // Assert
        result.Should().NotBeNull();
        result.Location.X.Should().Be(100);
        result.Location.Y.Should().Be(200);
        result.Label.Should().Be("Point A");
    }

    [Fact]
    public void Adapt_Struct_ZeroValues_ShouldMap()
    {
        // Arrange
        var source = new PointStructSource { X = 0, Y = 0 };

        // Act
        var result = source.Adapt<PointStructDest>();

        // Assert
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }

    [Fact]
    public void Adapt_Struct_NegativeValues_ShouldMap()
    {
        // Arrange
        var source = new PointStructSource { X = -50, Y = -100 };

        // Act
        var result = source.Adapt<PointStructDest>();

        // Assert
        result.X.Should().Be(-50);
        result.Y.Should().Be(-100);
    }

    [Fact]
    public void Adapt_Struct_MaxValues_ShouldMap()
    {
        // Arrange
        var source = new PointStructSource { X = int.MaxValue, Y = int.MinValue };

        // Act
        var result = source.Adapt<PointStructDest>();

        // Assert
        result.X.Should().Be(int.MaxValue);
        result.Y.Should().Be(int.MinValue);
    }

    [Fact]
    public void Adapt_DefaultStruct_ShouldMap()
    {
        // Arrange
        var source = new PointStructSource(); // default values

        // Act
        var result = source.Adapt<PointStructDest>();

        // Assert
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }

    [Fact]
    public void Adapt_StructWithinClass_MultipleInstances_ShouldMapIndependently()
    {
        // Arrange
        var source1 = new MixedStructSource
        {
            Location = new PointStructSource { X = 1, Y = 1 },
            Label = "First"
        };
        var source2 = new MixedStructSource
        {
            Location = new PointStructSource { X = 2, Y = 2 },
            Label = "Second"
        };

        // Act
        var result1 = source1.Adapt<MixedStructDest>();
        var result2 = source2.Adapt<MixedStructDest>();

        // Assert
        result1.Location.X.Should().Be(1);
        result1.Label.Should().Be("First");
        result2.Location.X.Should().Be(2);
        result2.Label.Should().Be("Second");
    }
}

