using Soenneker.Tests.Unit;
using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

[Collection("Collection")]
public sealed class NullableTests : UnitTest
{
    public NullableTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_NullableProperties_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NullablePropsSource
        {
            Name = "Test",
            Count = 42,
            Status = TestStatus.Active
        };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Count.Should().Be(42);
        result.Status.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void Adapt_NullableProperties_WithNulls_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NullablePropsSource
        {
            Name = null,
            Count = null,
            Status = null
        };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
        result.Count.Should().BeNull();
        result.Status.Should().BeNull();
    }

    [Fact]
    public void Adapt_NullableInt_WithValue_ShouldMap()
    {
        // Arrange
        var source = new NullablePropsSource { Count = 100 };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Count.Should().Be(100);
        result.Count.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Adapt_NullableInt_WithNull_ShouldMap()
    {
        // Arrange
        var source = new NullablePropsSource { Count = null };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Count.Should().BeNull();
        result.Count.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Adapt_NullableEnum_WithValue_ShouldMap()
    {
        // Arrange
        var source = new NullablePropsSource { Status = TestStatus.Pending };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Status.Should().Be(TestStatus.Pending);
        result.Status.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Adapt_NullableEnum_WithNull_ShouldMap()
    {
        // Arrange
        var source = new NullablePropsSource { Status = null };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Status.Should().BeNull();
        result.Status.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Adapt_ListWithNullableElements_ShouldCopy()
    {
        // Arrange
        var source = new List<int?> { 1, null, 3, null, 5 };

        // Act
        List<int?> result = source.Adapt();

        // Assert
        result.Count.Should().Be(5);
        result[0].Should().Be(1);
        result[1].Should().BeNull();
        result[2].Should().Be(3);
        result[3].Should().BeNull();
        result[4].Should().Be(5);
    }

    [Fact]
    public void Adapt_ListWithAllNulls_ShouldCopy()
    {
        // Arrange
        var source = new List<string> { null, null, null };

        // Act
        List<string> result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result[0].Should().BeNull();
        result[1].Should().BeNull();
        result[2].Should().BeNull();
    }

    [Fact]
    public void Adapt_DictionaryWithNullValues_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", null },
            { "key3", null }
        };

        // Act
        Dictionary<string, string> result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result["key1"].Should().Be("value1");
        result["key2"].Should().BeNull();
        result["key3"].Should().BeNull();
    }

    [Fact]
    public void Adapt_MixedNullableProperties_ShouldMapIndependently()
    {
        // Arrange - some properties null, some with values
        var source1 = new NullablePropsSource { Name = "Test", Count = null, Status = TestStatus.Active };
        var source2 = new NullablePropsSource { Name = null, Count = 42, Status = null };

        // Act
        var result1 = source1.Adapt<NullablePropsDest>();
        var result2 = source2.Adapt<NullablePropsDest>();

        // Assert
        result1.Name.Should().Be("Test");
        result1.Count.Should().BeNull();
        result1.Status.Should().Be(TestStatus.Active);

        result2.Name.Should().BeNull();
        result2.Count.Should().Be(42);
        result2.Status.Should().BeNull();
    }

    [Fact]
    public void OrderWithNullableCustomer_to_PersonRecordDest_should_adapt()
    {
        var nullableSource = AutoFaker.Generate<OrderWithNullableCustomer>();

        var clonedCustomer = nullableSource.Customer.Adapt<PersonRecordDest>();
        clonedCustomer.Should().NotBeNull();
    }

    [Fact]
    public void PersonRecordDest_to_OrderWithNullableCustomer_should_adapt()
    {
        var nullableSource = AutoFaker.Generate<PersonRecordDest>();

        var clonedCustomer = nullableSource.Adapt<PersonClass>();

        clonedCustomer.Should().NotBeNull();
    }

    [Fact]
    public void OrderWithNullablePerson_to_PersonClass_should_adapt()
    {
        var nullableSource = AutoFaker.Generate<OrderWithNullablePerson>();

        var clonedCustomer = nullableSource.Person.Adapt<PersonClass>();

        clonedCustomer.Should().NotBeNull();
    }
}

