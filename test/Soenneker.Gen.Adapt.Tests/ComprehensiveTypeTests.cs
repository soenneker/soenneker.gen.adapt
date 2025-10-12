using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class ComprehensiveTypeTests : UnitTest
{
    public ComprehensiveTypeTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_Arrays_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ArraySource
        {
            Names = new[] { "Alice", "Bob", "Charlie" },
            Numbers = new[] { 1, 2, 3, 4, 5 },
            People = new IPersonInterface[]
            {
                new PersonImplementation { FirstName = "John", LastName = "Doe", Age = 30 },
                new PersonImplementation { FirstName = "Jane", LastName = "Smith", Age = 25 }
            }
        };

        // Act
        var result = source.Adapt<ArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.Names.Should().NotBeNull();
        result.Names.Length.Should().Be(3);
        result.Names[0].Should().Be("Alice");
        result.Names[2].Should().Be("Charlie");

        result.Numbers.Should().NotBeNull();
        result.Numbers.Count.Should().Be(5);
        result.Numbers[0].Should().Be(1);
        result.Numbers[4].Should().Be(5);

        result.People.Should().NotBeNull();
        result.People.Length.Should().Be(2);
        result.People[0].FirstName.Should().Be("John");
        result.People[1].Age.Should().Be(25);
    }

    [Fact]
    public void Adapt_HashSet_ShouldMapCorrectly()
    {
        // Arrange
        var source = new HashSetSource
        {
            Tags = new HashSet<string> { "tag1", "tag2", "tag3" },
            Numbers = new HashSet<int> { 10, 20, 30 },
            People = new HashSet<IPersonInterface>
            {
                new PersonImplementation { FirstName = "Alice", LastName = "Wonder", Age = 35 }
            }
        };

        // Act
        var result = source.Adapt<HashSetDest>();

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().NotBeNull();
        result.Tags.Count.Should().Be(3);
        result.Tags.Should().Contain("tag1");

        result.Numbers.Should().NotBeNull();
        result.Numbers.Count.Should().Be(3);
        result.Numbers.Should().Contain(20);

        result.People.Should().NotBeNull();
        result.People.Count.Should().Be(1);
        result.People[0].FirstName.Should().Be("Alice");
        result.People[0].Age.Should().Be(35);
    }

    [Fact]
    public void Adapt_AllPrimitiveTypes_ShouldMapCorrectly()
    {
        // Arrange
        DateTimeOffset now = DateTimeOffset.UtcNow;
        TimeSpan span = TimeSpan.FromHours(5);
        
        var source = new AllPrimitivesSource
        {
            ByteValue = 255,
            ShortValue = 32767,
            LongValue = 9223372036854775807,
            FloatValue = 3.14f,
            DoubleValue = 3.141592653589793,
            CharValue = 'A',
            SByteValue = -128,
            UShortValue = 65535,
            UIntValue = 4294967295,
            ULongValue = 18446744073709551615,
            DateTimeOffsetValue = now,
            TimeSpanValue = span
        };

        // Act
        var result = source.Adapt<AllPrimitivesDest>();

        // Assert
        result.Should().NotBeNull();
        result.ByteValue.Should().Be(255);
        result.ShortValue.Should().Be(32767);
        result.LongValue.Should().Be(9223372036854775807);
        result.FloatValue.Should().Be(3.14f);
        result.DoubleValue.Should().Be(3.141592653589793);
        result.CharValue.Should().Be('A');
        result.SByteValue.Should().Be(-128);
        result.UShortValue.Should().Be(65535);
        result.UIntValue.Should().Be(4294967295);
        result.ULongValue.Should().Be(18446744073709551615);
        result.DateTimeOffsetValue.Should().Be(now);
        result.TimeSpanValue.Should().Be(span);
    }

    [Fact]
    public void Adapt_EmptyArrays_ShouldMapToEmptyCollections()
    {
        // Arrange
        var source = new ArraySource
        {
            Names = Array.Empty<string>(),
            Numbers = Array.Empty<int>(),
            People = Array.Empty<IPersonInterface>()
        };

        // Act
        var result = source.Adapt<ArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.Names.Length.Should().Be(0);
        result.Numbers.Count.Should().Be(0);
        result.People.Length.Should().Be(0);
    }

    [Fact]
    public void Adapt_LargeArray_ShouldMapAll()
    {
        // Arrange
        var numbers = new int[1000];
        for (int i = 0; i < 1000; i++)
            numbers[i] = i;

        var source = new ArraySource
        {
            Names = Array.Empty<string>(),
            Numbers = numbers,
            People = Array.Empty<IPersonInterface>()
        };

        // Act
        var result = source.Adapt<ArrayDest>();

        // Assert
        result.Numbers.Count.Should().Be(1000);
        result.Numbers[500].Should().Be(500);
        result.Numbers[999].Should().Be(999);
    }

    [Fact]
    public void Adapt_EmptyHashSet_ShouldMapCorrectly()
    {
        // Arrange
        var source = new HashSetSource
        {
            Tags = new HashSet<string>(),
            Numbers = new HashSet<int>(),
            People = new HashSet<IPersonInterface>()
        };

        // Act
        var result = source.Adapt<HashSetDest>();

        // Assert
        result.Should().NotBeNull();
        result.Tags.Count.Should().Be(0);
        result.Numbers.Count.Should().Be(0);
        result.People.Count.Should().Be(0);
    }
}

