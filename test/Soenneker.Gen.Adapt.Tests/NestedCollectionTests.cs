using Soenneker.Tests.Unit;
using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class NestedCollectionTests : UnitTest
{
    public NestedCollectionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_ListOfLists_Int_ShouldCopy()
    {
        // Arrange
        var source = new List<List<int>>
        {
            new() { 1, 2, 3 },
            new() { 4, 5, 6 },
            new() { 7, 8, 9 }
        };

        // Act
        var result = source.Adapt<List<List<int>>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result[0].Should().BeEquivalentTo([1, 2, 3]);
        result[1].Should().BeEquivalentTo([4, 5, 6]);
        result[2].Should().BeEquivalentTo([7, 8, 9]);
        // Should be shallow copy - inner lists are same references
        source[0].Should().BeSameAs(result[0]);
    }

    [Fact]
    public void Adapt_ListOfLists_String_ShouldCopy()
    {
        // Arrange
        var source = new List<List<string>>
        {
            new() { "a", "b" },
            new() { "c", "d" },
        };

        // Act
        var result = source.Adapt<List<List<string>>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].Should().BeEquivalentTo(new[] { "a", "b" });
        result[1].Should().BeEquivalentTo(new[] { "c", "d" });
    }

    [Fact]
    public void Adapt_ListOfDictionaries_ShouldCopy()
    {
        // Arrange
        var source = new List<Dictionary<string, int>>
        {
            new() { { "a", 1 }, { "b", 2 } },
            new() { { "c", 3 }, { "d", 4 } }
        };

        // Act
        var result = source.Adapt<List<Dictionary<string, int>>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0]["a"].Should().Be(1);
        result[1]["d"].Should().Be(4);
    }

    [Fact]
    public void Adapt_DictionaryOfLists_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, List<int>>
        {
            { "odds", [
                    1,
                    3,
                    5
                ]
            },
            { "evens", [
                    2,
                    4,
                    6
                ]
            }
        };

        // Act
        var result = source.Adapt<Dictionary<string, List<int>>>();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result["odds"].Should().BeEquivalentTo([1, 3, 5]);
        result["evens"].Should().BeEquivalentTo([2, 4, 6]);
    }

    [Fact]
    public void Adapt_ListOfNestedObjects_ShouldMapWithManualLoop()
    {
        // Arrange
        var source = new ComplexListSource
        {
            NestedItems =
            [
                new NestedSource
                {
                    Name = "Nested1",
                    Child = new BasicSource { Id = "1", Name = "Child1", Count = 10 }
                },

                new NestedSource
                {
                    Name = "Nested2",
                    Child = new BasicSource { Id = "2", Name = "Child2", Count = 20 }
                }
            ]
        };

        // Act
        var result = source.Adapt<ComplexListDest>();

        // Assert
        result.Should().NotBeNull();
        result.NestedItems.Should().NotBeNull();
        result.NestedItems.Count.Should().Be(2);
        result.NestedItems[0].Name.Should().Be("Nested1");
        result.NestedItems[0].Child.Id.Should().Be("1");
        result.NestedItems[1].Name.Should().Be("Nested2");
        result.NestedItems[1].Child.Id.Should().Be("2");
    }

    [Fact]
    public void Adapt_ListOfLists_Empty_ShouldCopy()
    {
        // Arrange
        var source = new List<List<int>>();

        // Act
        var result = source.Adapt<List<List<int>>>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_ListOfLists_WithEmptyInnerLists_ShouldCopy()
    {
        // Arrange
        var source = new List<List<int>>
        {
            new(),
            new() { 1, 2 },
            new()
        };

        // Act
        var result = source.Adapt<List<List<int>>>();

        // Assert
        result.Count.Should().Be(3);
        result[0].Should().BeEmpty();
        result[1].Should().BeEquivalentTo([1, 2]);
        result[2].Should().BeEmpty();
    }

    [Fact]
    public void Adapt_DictionaryOfDictionaries_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, Dictionary<string, int>>
        {
            { "group1", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
            { "group2", new Dictionary<string, int> { { "c", 3 } } }
        };

        // Act
        var result = source.Adapt<Dictionary<string, Dictionary<string, int>>>();

        // Assert
        result.Count.Should().Be(2);
        result["group1"]["a"].Should().Be(1);
        result["group1"]["b"].Should().Be(2);
        result["group2"]["c"].Should().Be(3);
    }

    [Fact]
    public void Adapt_ListOfLists_DifferentSizes_ShouldCopy()
    {
        // Arrange
        var source = new List<List<int>>
        {
            new() { 1 },
            new() { 2, 3, 4, 5 },
            new() { 6, 7 }
        };

        // Act
        var result = source.Adapt<List<List<int>>>();

        // Assert
        result.Count.Should().Be(3);
        result[0].Count.Should().Be(1);
        result[1].Count.Should().Be(4);
        result[2].Count.Should().Be(2);
    }

    [Fact]
    public void Adapt_TripleNestedLists_ShouldCopy()
    {
        // Arrange
        var source = new List<List<List<int>>>
        {
            new()
            {
                new List<int> { 1, 2 },
                new List<int> { 3, 4 }
            },
            new()
            {
                new List<int> { 5, 6 }
            }
        };

        // Act
        var result = source.Adapt<List<List<List<int>>>>();

        // Assert
        result.Count.Should().Be(2);
        result[0].Count.Should().Be(2);
        result[0][0].Should().BeEquivalentTo([1, 2]);
        result[0][1].Should().BeEquivalentTo([3, 4]);
        result[1].Count.Should().Be(1);
        result[1][0].Should().BeEquivalentTo([5, 6]);
    }

    [Fact]
    public void Adapt_ListOfDictionaryOfList_ShouldCopy()
    {
        // Arrange
        var source = new List<Dictionary<string, List<int>>>
        {
            new()
            {
                { "key1", [
                        1,
                        2,
                        3
                    ]
                }
            },
            new()
            {
                { "key2", [
                        4,
                        5
                    ]
                }
            }
        };

        // Act
        var result = source.Adapt<List<Dictionary<string, List<int>>>>();

        // Assert
        result.Count.Should().Be(2);
        result[0]["key1"].Should().BeEquivalentTo([1, 2, 3]);
        result[1]["key2"].Should().BeEquivalentTo([4, 5]);
    }
}

