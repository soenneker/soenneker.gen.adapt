using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Generics;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionGenericTests : UnitTest
{
    public ReflectionGenericTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_GenericWrapper_ConcreteIntType_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteIntWrapperSource
        {
            Value = 42,
            Name = "Integer Wrapper",
            AdditionalInfo = "Extra data"
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteIntWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(source.Value);
        result.Name.Should().Be(source.Name);
        result.AdditionalInfo.Should().Be(source.AdditionalInfo);
    }

    [Fact]
    public void AdaptViaReflection_GenericWrapper_ConcreteStringType_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteStringWrapperSource
        {
            Value = "Hello Generic",
            Name = "String Wrapper",
            IsActive = true
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteStringWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(source.Value);
        result.Name.Should().Be(source.Name);
        result.IsActive.Should().Be(source.IsActive);
    }

    [Fact]
    public void AdaptViaReflection_MultiGeneric_ConcreteType_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteKeyValueSource
        {
            Key = "test-key",
            Value = 999,
            Description = "Key-Value pair"
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteKeyValueDest>();

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be(source.Key);
        result.Value.Should().Be(source.Value);
        result.Description.Should().Be(source.Description);
    }

    [Fact]
    public void AdaptViaReflection_GenericWrapper_WithNullValue_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteStringWrapperSource
        {
            Value = null,
            Name = "Null Value Wrapper",
            IsActive = false
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteStringWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Name.Should().Be(source.Name);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AdaptViaReflection_GenericWrapper_WithDefaultIntValue_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteIntWrapperSource
        {
            Value = 0,
            Name = "Default Int",
            AdditionalInfo = null
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteIntWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(0);
        result.Name.Should().Be(source.Name);
        result.AdditionalInfo.Should().BeNull();
    }

    [Fact]
    public void AdaptViaReflection_MultiGeneric_WithEmptyString_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteKeyValueSource
        {
            Key = "",
            Value = 0,
            Description = ""
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteKeyValueDest>();

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().BeEmpty();
        result.Value.Should().Be(0);
        result.Description.Should().BeEmpty();
    }

    [Fact]
    public void AdaptViaReflection_ConcreteIntWrapper_LargeNumber_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteIntWrapperSource
        {
            Value = int.MaxValue,
            Name = "Max Int",
            AdditionalInfo = "Maximum integer value"
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteIntWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(int.MaxValue);
        result.Name.Should().Be(source.Name);
    }

    [Fact]
    public void AdaptViaReflection_ConcreteIntWrapper_NegativeNumber_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteIntWrapperSource
        {
            Value = int.MinValue,
            Name = "Min Int",
            AdditionalInfo = "Minimum integer value"
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteIntWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(int.MinValue);
    }

    [Fact]
    public void AdaptViaReflection_ConcreteStringWrapper_LongString_ShouldMapCorrectly()
    {
        // Arrange
        var longString = new string('A', 10000);
        var source = new ConcreteStringWrapperSource
        {
            Value = longString,
            Name = "Long String",
            IsActive = true
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteStringWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(longString);
        result.Value.Length.Should().Be(10000);
    }

    [Fact]
    public void AdaptViaReflection_ConcreteStringWrapper_SpecialCharacters_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteStringWrapperSource
        {
            Value = "Special: \n\t\r\"'\\/@#$%^&*()",
            Name = "Special Chars",
            IsActive = true
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteStringWrapperDest>();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(source.Value);
    }

    [Fact]
    public void AdaptViaReflection_MultipleGenericInstances_WithCaching_ShouldMapCorrectly()
    {
        // Arrange
        var source1 = new ConcreteIntWrapperSource { Value = 1, Name = "One", AdditionalInfo = "First" };
        var source2 = new ConcreteIntWrapperSource { Value = 2, Name = "Two", AdditionalInfo = "Second" };
        var source3 = new ConcreteIntWrapperSource { Value = 3, Name = "Three", AdditionalInfo = "Third" };

        // Act - Should benefit from mapper caching
        var result1 = source1.AdaptViaReflection<ConcreteIntWrapperDest>();
        var result2 = source2.AdaptViaReflection<ConcreteIntWrapperDest>();
        var result3 = source3.AdaptViaReflection<ConcreteIntWrapperDest>();

        // Assert
        result1.Value.Should().Be(1);
        result1.Name.Should().Be("One");
        result2.Value.Should().Be(2);
        result2.Name.Should().Be("Two");
        result3.Value.Should().Be(3);
        result3.Name.Should().Be("Three");
    }

    [Fact]
    public void AdaptViaReflection_MultiGeneric_WithNegativeValue_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ConcreteKeyValueSource
        {
            Key = "negative",
            Value = -999,
            Description = "Negative value test"
        };

        // Act
        var result = source.AdaptViaReflection<ConcreteKeyValueDest>();

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be(source.Key);
        result.Value.Should().Be(-999);
        result.Description.Should().Be(source.Description);
    }
}

