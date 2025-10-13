using System;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Primitives;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionPrimitivesTests : UnitTest
{
    public ReflectionPrimitivesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_AllPrimitiveTypes_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            BoolValue = true,
            ByteValue = 255,
            SByteValue = -128,
            CharValue = 'A',
            ShortValue = -32768,
            UShortValue = 65535,
            IntValue = 42,
            UIntValue = 4294967295,
            LongValue = -9223372036854775808,
            ULongValue = 18446744073709551615,
            FloatValue = 3.14f,
            DoubleValue = 2.718281828,
            DecimalValue = 123.456m,
            StringValue = "Hello, World!",
            DateTimeValue = new DateTime(2024, 1, 15, 10, 30, 0),
            DateTimeOffsetValue = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.FromHours(-5)),
            TimeSpanValue = TimeSpan.FromHours(5.5),
            GuidValue = Guid.Parse("12345678-1234-1234-1234-123456789012")
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
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
    public void AdaptViaReflection_AllPrimitiveTypes_WithDefaultValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            BoolValue = false,
            ByteValue = 0,
            SByteValue = 0,
            CharValue = '\0',
            ShortValue = 0,
            UShortValue = 0,
            IntValue = 0,
            UIntValue = 0,
            LongValue = 0,
            ULongValue = 0,
            FloatValue = 0f,
            DoubleValue = 0d,
            DecimalValue = 0m,
            StringValue = string.Empty,
            DateTimeValue = DateTime.MinValue,
            DateTimeOffsetValue = DateTimeOffset.MinValue,
            TimeSpanValue = TimeSpan.Zero,
            GuidValue = Guid.Empty
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.Should().NotBeNull();
        result.BoolValue.Should().BeFalse();
        result.ByteValue.Should().Be(0);
        result.IntValue.Should().Be(0);
        result.StringValue.Should().BeEmpty();
        result.GuidValue.Should().Be(Guid.Empty);
    }

    [Fact]
    public void AdaptViaReflection_NullablePrimitives_WithValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NullablePrimitivesSource
        {
            BoolValue = true,
            IntValue = 42,
            LongValue = 1000000000L,
            DoubleValue = 3.14159,
            DecimalValue = 99.99m,
            DateTimeValue = new DateTime(2024, 6, 15),
            GuidValue = Guid.NewGuid()
        };

        // Act
        var result = source.AdaptViaReflection<NullablePrimitivesDest>();

        // Assert
        result.Should().NotBeNull();
        result.BoolValue.Should().Be(source.BoolValue);
        result.IntValue.Should().Be(source.IntValue);
        result.LongValue.Should().Be(source.LongValue);
        result.DoubleValue.Should().Be(source.DoubleValue);
        result.DecimalValue.Should().Be(source.DecimalValue);
        result.DateTimeValue.Should().Be(source.DateTimeValue);
        result.GuidValue.Should().Be(source.GuidValue);
    }

    [Fact]
    public void AdaptViaReflection_NullablePrimitives_WithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NullablePrimitivesSource
        {
            BoolValue = null,
            IntValue = null,
            LongValue = null,
            DoubleValue = null,
            DecimalValue = null,
            DateTimeValue = null,
            GuidValue = null
        };

        // Act
        var result = source.AdaptViaReflection<NullablePrimitivesDest>();

        // Assert
        result.Should().NotBeNull();
        result.BoolValue.Should().BeNull();
        result.IntValue.Should().BeNull();
        result.LongValue.Should().BeNull();
        result.DoubleValue.Should().BeNull();
        result.DecimalValue.Should().BeNull();
        result.DateTimeValue.Should().BeNull();
        result.GuidValue.Should().BeNull();
    }

    [Fact]
    public void AdaptViaReflection_MixedNullable_ShouldMapCorrectly()
    {
        // Arrange
        var source = new MixedNullableSource
        {
            NonNullableInt = 100,
            NullableInt = 200,
            NonNullableString = "Test",
            NonNullableDateTime = new DateTime(2024, 1, 1),
            NullableDateTime = new DateTime(2024, 12, 31)
        };

        // Act
        var result = source.AdaptViaReflection<MixedNullableDest>();

        // Assert
        result.Should().NotBeNull();
        result.NonNullableInt.Should().Be(source.NonNullableInt);
        result.NullableInt.Should().Be(source.NullableInt);
        result.NonNullableString.Should().Be(source.NonNullableString);
        result.NonNullableDateTime.Should().Be(source.NonNullableDateTime);
        result.NullableDateTime.Should().Be(source.NullableDateTime);
    }

    [Fact]
    public void AdaptViaReflection_MixedNullable_WithNullValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new MixedNullableSource
        {
            NonNullableInt = 0,
            NullableInt = null,
            NonNullableString = null,
            NonNullableDateTime = DateTime.MinValue,
            NullableDateTime = null
        };

        // Act
        var result = source.AdaptViaReflection<MixedNullableDest>();

        // Assert
        result.Should().NotBeNull();
        result.NonNullableInt.Should().Be(0);
        result.NullableInt.Should().BeNull();
        result.NonNullableString.Should().BeNull();
        result.NonNullableDateTime.Should().Be(DateTime.MinValue);
        result.NullableDateTime.Should().BeNull();
    }

    [Fact]
    public void AdaptViaReflection_StringProperties_WithSpecialCharacters_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ReflectionAllPrimitivesSource
        {
            StringValue = "Special chars: \n\t\r\"'\\/@#$%^&*()"
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.StringValue.Should().Be(source.StringValue);
    }

    [Fact]
    public void AdaptViaReflection_DateTimeEdgeCases_ShouldMapCorrectly()
    {
        // Arrange - Test Min, Max, and UTC values
        var source = new ReflectionAllPrimitivesSource
        {
            DateTimeValue = DateTime.MaxValue
        };

        // Act
        var result = source.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result.DateTimeValue.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void AdaptViaReflection_GuidValues_MultipleTimes_ShouldBeDifferent()
    {
        // Arrange
        var source1 = new ReflectionAllPrimitivesSource { GuidValue = Guid.NewGuid() };
        var source2 = new ReflectionAllPrimitivesSource { GuidValue = Guid.NewGuid() };

        // Act
        var result1 = source1.AdaptViaReflection<ReflectionAllPrimitivesDest>();
        var result2 = source2.AdaptViaReflection<ReflectionAllPrimitivesDest>();

        // Assert
        result1.GuidValue.Should().Be(source1.GuidValue);
        result2.GuidValue.Should().Be(source2.GuidValue);
        result1.GuidValue.Should().NotBe(result2.GuidValue);
    }
}

