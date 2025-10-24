using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System;
using System.Collections.Generic;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

/// <summary>
/// Tests for simple array support in the Gen.Adapt library.
/// Covers basic array mapping scenarios, primitive types, nullable types,
/// and various array-to-collection mapping patterns.
/// </summary>
public sealed class SimpleArrayTests : UnitTest
{
    public SimpleArrayTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_IntArray_primitive_ShouldMapCorrectly()
    {
        // Arrange
        var source = new int[] { 1, 2, 3, 4, 5 };

        // Act
        var result = source.Adapt<int[]>();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeNull();
        result.Length.Should().Be(2);
        result[4].Should().Be(5);
        result.Should().Equal([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Adapt_IntArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            IntArray = [1, 2, 3, 4, 5]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().NotBeNull();
        result.IntArray.Length.Should().Be(5);
        result.IntArray[0].Should().Be(1);
        result.IntArray[4].Should().Be(5);
        result.IntArray.Should().Equal([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Adapt_StringArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            StringArray = ["alpha", "beta", "gamma", "delta"]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.StringArray.Should().NotBeNull();
        result.StringArray.Length.Should().Be(4);
        result.StringArray[0].Should().Be("alpha");
        result.StringArray[3].Should().Be("delta");
        result.StringArray.Should().Equal(["alpha", "beta", "gamma", "delta"]);
    }

    [Fact]
    public void Adapt_DoubleArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            DoubleArray = [1.1, 2.2, 3.3, 4.4, 5.5]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.DoubleArray.Should().NotBeNull();
        result.DoubleArray.Length.Should().Be(5);
        result.DoubleArray[0].Should().Be(1.1);
        result.DoubleArray[4].Should().Be(5.5);
        result.DoubleArray.Should().Equal([1.1, 2.2, 3.3, 4.4, 5.5]);
    }

    [Fact]
    public void Adapt_BoolArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            BoolArray = [true, false, true, false, true]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.BoolArray.Should().NotBeNull();
        result.BoolArray.Length.Should().Be(5);
        result.BoolArray[0].Should().BeTrue();
        result.BoolArray[1].Should().BeFalse();
        result.BoolArray[4].Should().BeTrue();
        result.BoolArray.Should().Equal([true, false, true, false, true]);
    }

    [Fact]
    public void Adapt_CharArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            CharArray = ['A', 'B', 'C', 'D', 'E']
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.CharArray.Should().NotBeNull();
        result.CharArray.Length.Should().Be(5);
        result.CharArray[0].Should().Be('A');
        result.CharArray[4].Should().Be('E');
        result.CharArray.Should().Equal(['A', 'B', 'C', 'D', 'E']);
    }

    [Fact]
    public void Adapt_ByteArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            ByteArray = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.ByteArray.Should().NotBeNull();
        result.ByteArray.Length.Should().Be(10);
        result.ByteArray[0].Should().Be(0);
        result.ByteArray[9].Should().Be(9);
        result.ByteArray.Should().Equal([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Adapt_EmptyArrays_ShouldMapToEmptyArrays()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            IntArray = [],
            StringArray = [],
            DoubleArray = [],
            BoolArray = [],
            CharArray = [],
            ByteArray = []
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().NotBeNull();
        result.IntArray.Length.Should().Be(0);
        result.StringArray.Should().NotBeNull();
        result.StringArray.Length.Should().Be(0);
        result.DoubleArray.Should().NotBeNull();
        result.DoubleArray.Length.Should().Be(0);
        result.BoolArray.Should().NotBeNull();
        result.BoolArray.Length.Should().Be(0);
        result.CharArray.Should().NotBeNull();
        result.CharArray.Length.Should().Be(0);
        result.ByteArray.Should().NotBeNull();
        result.ByteArray.Length.Should().Be(0);
    }

    [Fact]
    public void Adapt_SingleElementArrays_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            IntArray = [42],
            StringArray = ["single"],
            DoubleArray = [3.14],
            BoolArray = [true],
            CharArray = ['X'],
            ByteArray = [255]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Length.Should().Be(1);
        result.IntArray[0].Should().Be(42);
        result.StringArray.Length.Should().Be(1);
        result.StringArray[0].Should().Be("single");
        result.DoubleArray.Length.Should().Be(1);
        result.DoubleArray[0].Should().Be(3.14);
        result.BoolArray.Length.Should().Be(1);
        result.BoolArray[0].Should().BeTrue();
        result.CharArray.Length.Should().Be(1);
        result.CharArray[0].Should().Be('X');
        result.ByteArray.Length.Should().Be(1);
        result.ByteArray[0].Should().Be(255);
    }

    [Fact]
    public void Adapt_NullableIntArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            NullableIntArray = [1, null, 3, null, 5]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.NullableIntArray.Should().NotBeNull();
        result.NullableIntArray.Length.Should().Be(5);
        result.NullableIntArray[0].Should().Be(1);
        result.NullableIntArray[1].Should().BeNull();
        result.NullableIntArray[2].Should().Be(3);
        result.NullableIntArray[3].Should().BeNull();
        result.NullableIntArray[4].Should().Be(5);
    }

    [Fact]
    public void Adapt_NullableStringArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            NullableStringArray = ["first", null, "third", null, "fifth"]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.NullableStringArray.Should().NotBeNull();
        result.NullableStringArray.Length.Should().Be(5);
        result.NullableStringArray[0].Should().Be("first");
        result.NullableStringArray[1].Should().BeNull();
        result.NullableStringArray[2].Should().Be("third");
        result.NullableStringArray[3].Should().BeNull();
        result.NullableStringArray[4].Should().Be("fifth");
    }

    [Fact]
    public void Adapt_ArrayToCollection_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            IntArray = [10, 20, 30, 40, 50]
        };

        // Act
        var result = source.Adapt<ArrayToCollectionDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().NotBeNull();
        result.IntArray.Count.Should().Be(5);
        result.IntArray[0].Should().Be(10);
        result.IntArray[4].Should().Be(50);
        result.IntArray.Should().Equal([10, 20, 30, 40, 50]);
    }

    [Fact]
    public void Adapt_CollectionToArray_ShouldMapCorrectly()
    {
        // Arrange
        var source = new CollectionToArraySource
        {
            IntArray = [100, 200, 300, 400, 500]
        };

        // Act
        var result = source.Adapt<CollectionToArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().NotBeNull();
        result.IntArray.Length.Should().Be(5);
        result.IntArray[0].Should().Be(100);
        result.IntArray[4].Should().Be(500);
        result.IntArray.Should().Equal([100, 200, 300, 400, 500]);
    }

    [Fact]
    public void Adapt_LargeArray_ShouldMapAllElements()
    {
        // Arrange
        var largeArray = new int[1000];
        for (var i = 0; i < 1000; i++)
            largeArray[i] = i;

        var source = new SimpleArraySource
        {
            IntArray = largeArray
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().NotBeNull();
        result.IntArray.Length.Should().Be(1000);
        result.IntArray[0].Should().Be(0);
        result.IntArray[500].Should().Be(500);
        result.IntArray[999].Should().Be(999);
    }

    [Fact]
    public void Adapt_ArrayWithNegativeValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            IntArray = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().NotBeNull();
        result.IntArray.Length.Should().Be(11);
        result.IntArray[0].Should().Be(-5);
        result.IntArray[5].Should().Be(0);
        result.IntArray[10].Should().Be(5);
        result.IntArray.Should().Equal([-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Adapt_ArrayWithDecimalValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            DoubleArray = [-3.14, -2.71, -1.41, 0.0, 1.41, 2.71, 3.14]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.DoubleArray.Should().NotBeNull();
        result.DoubleArray.Length.Should().Be(7);
        result.DoubleArray[0].Should().Be(-3.14);
        result.DoubleArray[3].Should().Be(0.0);
        result.DoubleArray[6].Should().Be(3.14);
        result.DoubleArray.Should().Equal([-3.14, -2.71, -1.41, 0.0, 1.41, 2.71, 3.14]);
    }

    [Fact]
    public void Adapt_ArrayWithSpecialCharacters_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SimpleArraySource
        {
            StringArray = ["Hello, World!", "Café", "测试", "🚀", "Line\nBreak", "Tab\tCharacter"]
        };

        // Act
        var result = source.Adapt<SimpleArrayDest>();

        // Assert
        result.Should().NotBeNull();
        result.StringArray.Should().NotBeNull();
        result.StringArray.Length.Should().Be(6);
        result.StringArray[0].Should().Be("Hello, World!");
        result.StringArray[1].Should().Be("Café");
        result.StringArray[2].Should().Be("测试");
        result.StringArray[3].Should().Be("🚀");
        result.StringArray[4].Should().Be("Line\nBreak");
        result.StringArray[5].Should().Be("Tab\tCharacter");
    }
}
