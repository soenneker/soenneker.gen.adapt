using System;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Primitives;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Collections;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionEdgeCasesTests : UnitTest
{
    public ReflectionEdgeCasesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_NullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        ReflectionAllPrimitivesSource? source = null;

        // Act & Assert
        Action act = () => source!.AdaptViaReflection<ReflectionAllPrimitivesDest>();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AdaptViaReflection_EmptyObject_ShouldCreateEmptyDestination()
    {
        // Arrange
        var source = new BasicSource();

        // Act
        var result = source.AdaptViaReflection<BasicDest>();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void AdaptViaReflection_SameSourceAndDest_ShouldCopyAllProperties()
    {
        // Arrange
        var source = new BasicSource
        {
            Id = "same-id",
            Name = "Same Name",
            Count = 123
        };

        // Act
        var result = source.AdaptViaReflection<BasicSource>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        result.Count.Should().Be(source.Count);
        result.Should().NotBeSameAs(source); // Should be a different instance
    }

    [Fact]
    public void AdaptViaReflection_PropertiesWithDifferentTypes_ShouldOnlyCopyCompatible()
    {
        // Arrange
        var source = new PartialMatchSource
        {
            Id = "partial",
            Name = "Test",
            ExtraField = "Not copied"
        };

        // Act
        var result = source.AdaptViaReflection<PartialMatchDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
    }

    [Fact]
    public void AdaptViaReflection_WithReadOnlyProperties_ShouldSkipReadOnly()
    {
        // Arrange - Properties without setters won't be copied
        var source = new BasicSource
        {
            Id = "readonly-test",
            Name = "Test Name",
            Count = 42
        };

        // Act
        var result = source.AdaptViaReflection<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        result.Count.Should().Be(source.Count);
    }

    [Fact]
    public void AdaptViaReflection_MinAndMaxValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            ByteValue = byte.MaxValue,
            SByteValue = sbyte.MinValue,
            ShortValue = short.MinValue,
            UShortValue = ushort.MaxValue,
            IntValue = int.MaxValue,
            UIntValue = uint.MaxValue,
            LongValue = long.MinValue,
            ULongValue = ulong.MaxValue,
            FloatValue = float.MaxValue,
            DoubleValue = double.MinValue,
            DecimalValue = decimal.MaxValue
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.ByteValue.Should().Be(byte.MaxValue);
        result.SByteValue.Should().Be(sbyte.MinValue);
        result.ShortValue.Should().Be(short.MinValue);
        result.UShortValue.Should().Be(ushort.MaxValue);
        result.IntValue.Should().Be(int.MaxValue);
        result.UIntValue.Should().Be(uint.MaxValue);
        result.LongValue.Should().Be(long.MinValue);
        result.ULongValue.Should().Be(ulong.MaxValue);
        result.FloatValue.Should().Be(float.MaxValue);
        result.DoubleValue.Should().Be(double.MinValue);
        result.DecimalValue.Should().Be(decimal.MaxValue);
    }

    [Fact]
    public void AdaptViaReflection_SpecialFloatingPointValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            FloatValue = float.NaN,
            DoubleValue = double.PositiveInfinity
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        float.IsNaN(result.FloatValue).Should().BeTrue();
        double.IsPositiveInfinity(result.DoubleValue).Should().BeTrue();
    }

    [Fact]
    public void AdaptViaReflection_EmptyGuid_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            GuidValue = Guid.Empty
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.GuidValue.Should().Be(Guid.Empty);
    }

    [Fact]
    public void AdaptViaReflection_UtcDateTime_ShouldPreserveKind()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        var source = new ReflectionAllPrimitivesSource
        {
            DateTimeValue = utcNow
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.DateTimeValue.Should().Be(utcNow);
        result.DateTimeValue.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void AdaptViaReflection_VeryLongString_ShouldMapCorrectly()
    {
        // Arrange
        var veryLongString = new string('X', 1_000_000);
        var source = new ReflectionAllPrimitivesSource
        {
            StringValue = veryLongString
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.StringValue.Should().Be(veryLongString);
        result.StringValue.Length.Should().Be(1_000_000);
    }

    [Fact]
    public void AdaptViaReflection_UnicodeString_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            StringValue = "Hello 世界 🌍 Привет مرحبا"
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.StringValue.Should().Be(source.StringValue);
    }

    [Fact]
    public void AdaptViaReflection_WhitespaceString_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            StringValue = "   \t\n\r   "
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.StringValue.Should().Be(source.StringValue);
    }

    [Fact]
    public void AdaptViaReflection_CollectionWithNulls_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ListCollectionsSource
        {
            StringList =
            [
                "one",
                null,
                "three",
                null,
                "five"
            ]
        };

        // Act
        var result = source.AdaptViaReflection<ListCollectionsDest>();

        // Assert
        result.StringList.Should().NotBeNull();
        result.StringList.Count.Should().Be(5);
        result.StringList[0].Should().Be("one");
        result.StringList[1].Should().BeNull();
        result.StringList[2].Should().Be("three");
    }

    [Fact]
    public void AdaptViaReflection_SinglePropertyObject_ShouldMapCorrectly()
    {
        // Arrange
        var source = new CircularReferenceSource
        {
            Id = 1,
            Name = "Single"
        };

        // Act
        var result = source.AdaptViaReflection<CircularReferenceDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Single");
    }

    [Fact]
    public void AdaptViaReflection_ManyProperties_ShouldMapAll()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            BoolValue = true,
            ByteValue = 1,
            SByteValue = -1,
            CharValue = 'Z',
            ShortValue = 100,
            UShortValue = 200,
            IntValue = 1000,
            UIntValue = 2000,
            LongValue = 10000,
            ULongValue = 20000,
            FloatValue = 1.5f,
            DoubleValue = 2.5,
            DecimalValue = 3.5m,
            StringValue = "Test",
            DateTimeValue = DateTime.Now,
            DateTimeOffsetValue = DateTimeOffset.Now,
            TimeSpanValue = TimeSpan.FromMinutes(30),
            GuidValue = Guid.NewGuid()
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert - All 18 properties should be mapped
        result.Should().NotBeNull();
        result.BoolValue.Should().Be(source.BoolValue);
        result.ByteValue.Should().Be(source.ByteValue);
        result.SByteValue.Should().Be(source.SByteValue);
        result.CharValue.Should().Be(source.CharValue);
        result.ShortValue.Should().Be(source.ShortValue);
        result.UShortValue.Should().Be(source.UShortValue);
        result.IntValue.Should().Be(source.IntValue);
        result.UIntValue.Should().Be(source.UIntValue);
        result.LongValue.Should().Be(source.LongValue);
        result.ULongValue.Should().Be(source.ULongValue);
        result.FloatValue.Should().Be(source.FloatValue);
        result.DoubleValue.Should().Be(source.DoubleValue);
        result.DecimalValue.Should().Be(source.DecimalValue);
        result.StringValue.Should().Be(source.StringValue);
        result.DateTimeValue.Should().Be(source.DateTimeValue);
        result.DateTimeOffsetValue.Should().Be(source.DateTimeOffsetValue);
        result.TimeSpanValue.Should().Be(source.TimeSpanValue);
        result.GuidValue.Should().Be(source.GuidValue);
    }

    [Fact]
    public void AdaptViaReflection_SequentialCalls_DifferentTypes_ShouldMapCorrectly()
    {
        // Arrange
        var primitiveSource = new ReflectionAllPrimitivesSource { IntValue = 100 };
        var listSource = new ListCollectionsSource { IntList =
            [
                1,
                2,
                3
            ]
        };
        var nestedSource = new NestedObjectSource { Name = "Nested" };

        // Act - Different type pairs should each cache their own mappers
        var primitiveResult = primitiveSource.AdaptViaReflection<ReflectionAllPrimitivesDest>();
        var listResult = listSource.AdaptViaReflection<ListCollectionsDest>();
        var nestedResult = nestedSource.AdaptViaReflection<NestedObjectDest>();

        // Assert
        primitiveResult.IntValue.Should().Be(100);
        listResult.IntList.Count.Should().Be(3);
        nestedResult.Name.Should().Be("Nested");
    }

    [Fact]
    public void AdaptViaReflection_TimeSpanValues_EdgeCases_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            TimeSpanValue = TimeSpan.MaxValue
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.TimeSpanValue.Should().Be(TimeSpan.MaxValue);
    }

    [Fact]
    public void AdaptViaReflection_DefaultChar_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            CharValue = '\0'
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.CharValue.Should().Be('\0');
    }
}

