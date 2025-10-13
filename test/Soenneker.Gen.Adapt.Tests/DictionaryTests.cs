using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class DictionaryTests : UnitTest
{
    public DictionaryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_Dictionary_StringString_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        Dictionary<string, string> result = source.Adapt<Dictionary<string, string>>();

        // Assert
        result.Should().NotBeNull();
        source.Should().NotBeSameAs(result);
        result.Count.Should().Be(3);
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
        result["key3"].Should().Be("value3");
    }

    [Fact]
    public void Adapt_Dictionary_IntString_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" }
        };

        // Act
        Dictionary<int, string> result = source.Adapt< Dictionary<int, string>>();

        // Assert
        result.Should().NotBeNull();
        source.Should().NotBeSameAs(result);
        result.Count.Should().Be(3);
        result[1].Should().Be("one");
        result[2].Should().Be("two");
        result[3].Should().Be("three");
    }

    [Fact]
    public void Adapt_Dictionary_StringInt_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        // Act
        Dictionary<string, int> result = source.Adapt<Dictionary<string, int>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result["one"].Should().Be(1);
        result["two"].Should().Be(2);
        result["three"].Should().Be(3);
    }

    [Fact]
    public void Adapt_Dictionary_Empty_ShouldReturnEmpty()
    {
        // Arrange
        var source = new Dictionary<string, int>();

        // Act
        Dictionary<string, int> result = source.Adapt<Dictionary<string, int>>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_IDictionary_ShouldCopy()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int>
        {
            { "a", 100 },
            { "b", 200 }
        };

        // Act
        Dictionary<string, int> result = source.Adapt<Dictionary<string, int>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result["a"].Should().Be(100);
        result["b"].Should().Be(200);
    }

    [Fact]
    public void Adapt_Dictionary_ComplexKey_Guid_ShouldCopy()
    {
        // Arrange
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var source = new Dictionary<Guid, string>
        {
            { key1, "first" },
            { key2, "second" }
        };

        // Act
        Dictionary<Guid, string> result = source.Adapt<Dictionary<Guid, string>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[key1].Should().Be("first");
        result[key2].Should().Be("second");
    }

    [Fact]
    public void Adapt_Dictionary_LargeCollection_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<int, string>();
        for (int i = 0; i < 500; i++)
        {
            source[i] = $"value_{i}";
        }

        // Act
        Dictionary<int, string> result = source.Adapt<Dictionary<int, string>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(500);
        result[0].Should().Be("value_0");
        result[499].Should().Be("value_499");
    }

    [Fact]
    public void Adapt_Dictionary_IntInt_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<int, int>
        {
            { 1, 10 },
            { 2, 20 },
            { 3, 30 }
        };

        // Act
        Dictionary<int, int> result = source.Adapt<Dictionary<int, int>>();

        // Assert
        result.Count.Should().Be(3);
        result[1].Should().Be(10);
        result[2].Should().Be(20);
        result[3].Should().Be(30);
    }

    [Fact]
    public void Adapt_Dictionary_LongLong_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<long, long>
        {
            { 1000000000L, 9000000000L },
            { 2000000000L, 8000000000L }
        };

        // Act
        Dictionary<long, long> result = source.Adapt<Dictionary<long, long>>();

        // Assert
        result.Count.Should().Be(2);
        result[1000000000L].Should().Be(9000000000L);
    }

    [Fact]
    public void Adapt_Dictionary_BoolString_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<bool, string>
        {
            { true, "yes" },
            { false, "no" }
        };

        // Act
        Dictionary<bool, string> result = source.Adapt<Dictionary<bool, string>>();

        // Assert
        result.Count.Should().Be(2);
        result[true].Should().Be("yes");
        result[false].Should().Be("no");
    }

    [Fact]
    public void Adapt_Dictionary_WithNullValues_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", null },
            { "key3", "value3" }
        };

        // Act
        Dictionary<string, string> result = source.Adapt<Dictionary<string, string>>();

        // Assert
        result.Count.Should().Be(3);
        result["key1"].Should().Be("value1");
        result["key2"].Should().BeNull();
        result["key3"].Should().Be("value3");
    }

    [Fact]
    public void Adapt_Dictionary_SingleEntry_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, int> { { "solo", 42 } };

        // Act
        Dictionary<string, int> result = source.Adapt<Dictionary<string, int>>();

        // Assert
        result.Count.Should().Be(1);
        result["solo"].Should().Be(42);
    }

    [Fact]
    public void Adapt_Dictionary_DateTimeKeys_ShouldCopy()
    {
        // Arrange
        DateTime date1 = DateTime.UtcNow;
        DateTime date2 = date1.AddDays(1);
        var source = new Dictionary<DateTime, string>
        {
            { date1, "today" },
            { date2, "tomorrow" }
        };

        // Act
        Dictionary<DateTime, string> result = source.Adapt< Dictionary<DateTime, string>>();

        // Assert
        result.Count.Should().Be(2);
        result[date1].Should().Be("today");
        result[date2].Should().Be("tomorrow");
    }

    [Fact]
    public void Adapt_Dictionary_DecimalValues_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, decimal>
        {
            { "price1", 19.99m },
            { "price2", 99.99m },
            { "price3", 0.01m }
        };

        // Act
        Dictionary<string, decimal> result = source.Adapt<Dictionary<string, decimal>>();

        // Assert
        result.Count.Should().Be(3);
        result["price1"].Should().Be(19.99m);
        result["price2"].Should().Be(99.99m);
        result["price3"].Should().Be(0.01m);
    }
}

