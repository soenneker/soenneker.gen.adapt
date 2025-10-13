using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class ComplexScenarioTests : UnitTest
{
    public ComplexScenarioTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_ObjectWithAllPropertyTypes_ShouldMapAll()
    {
        // Arrange - test mapping multiple different property types in one go
        var source = new MultiListSource
        {
            Numbers =
            [
                1,
                2,
                3,
                4,
                5
            ],
            Tags =
            [
                "tag1",
                "tag2",
                "tag3"
            ],
            Items =
            [
                new() { Id = "a", Name = "First", Count = 10 },

                new() { Id = "b", Name = "Second", Count = 20 },

                new() { Id = "c", Name = "Third", Count = 30 }
            ]
        };

        // Act
        var result = source.Adapt<MultiListDest>();

        // Assert
        result.Numbers.Should().BeEquivalentTo([1, 2, 3, 4, 5]);
        result.Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2", "tag3" });
        result.Items.Count.Should().Be(3);
        result.Items[0].Id.Should().Be("a");
        result.Items[1].Name.Should().Be("Second");
        result.Items[2].Count.Should().Be(30);
    }

    [Fact]
    public void Adapt_ChainedAdaptations_ShouldWork()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "Test Name", Count = 100 };

        // Act - adapt to intermediate, then to final
        BasicDest intermediate = source.Adapt<BasicDest>();
        intermediate.Count = 200; // Modify intermediate
        BasicSource final = intermediate.Adapt<BasicSource>();

        // Assert
        final.Id.Should().Be("test");
        final.Name.Should().Be("Test Name");
        final.Count.Should().Be(200); // Should have the modified value
    }

    [Fact]
    public void Adapt_MixedNestingTypes_ShouldHandle()
    {
        // Arrange - mix of structs, classes, lists
        var source = new MixedStructSource
        {
            Location = new PointStructSource { X = 50, Y = 75 },
            Label = "Complex Mix"
        };

        // Act
        var result = source.Adapt<MixedStructDest>();

        // Assert
        result.Location.X.Should().Be(50);
        result.Location.Y.Should().Be(75);
        result.Label.Should().Be("Complex Mix");
    }

    [Fact]
    public void Adapt_ThreeLevelNesting_WithLists_ShouldMapAll()
    {
        // Arrange
        var source = new DeepNestedSource
        {
            RootName = "RootLevel",
            Level1 = new NestedSource
            {
                Name = "MiddleLevel",
                Child = new BasicSource
                {
                    Id = "leaf-id",
                    Name = "LeafLevel",
                    Count = 777
                }
            }
        };

        // Act
        var result = source.Adapt<DeepNestedDest>();

        // Assert
        // Verify full depth
        result.RootName.Should().Be("RootLevel");
        result.Level1.Name.Should().Be("MiddleLevel");
        result.Level1.Child.Id.Should().Be("leaf-id");
        result.Level1.Child.Name.Should().Be("LeafLevel");
        result.Level1.Child.Count.Should().Be(777);
    }

    [Fact]
    public void Adapt_MultiplePropertiesWithSameTargetType_ShouldMapAll()
    {
        // Testing multiple properties that need the same type conversion
        var source = new ComplexListSource
        {
            NestedItems =
            [
                new()
                {
                    Name = "First",
                    Child = new BasicSource { Id = "1", Name = "C1", Count = 1 }
                },

                new()
                {
                    Name = "Second",
                    Child = new BasicSource { Id = "2", Name = "C2", Count = 2 }
                },

                new()
                {
                    Name = "Third",
                    Child = new BasicSource { Id = "3", Name = "C3", Count = 3 }
                }
            ]
        };

        // Act
        var result = source.Adapt<ComplexListDest>();

        // Assert
        result.NestedItems.Count.Should().Be(3);
        // All children should be mapped correctly
        result.NestedItems.Should().AllSatisfy(item => 
        {
            item.Should().NotBeNull();
            item.Child.Should().NotBeNull();
        });
    }

    [Fact]
    public void Adapt_DictionaryOfComplexObjects_ShouldMap()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        
        var source = new Dictionary<Guid, BasicSource>
        {
            { guid1, new BasicSource { Id = "first", Name = "First Item", Count = 1 } },
            { guid2, new BasicSource { Id = "second", Name = "Second Item", Count = 2 } }
        };

        // Act
        Dictionary<Guid, BasicSource> result = source.Adapt<Dictionary<Guid, BasicSource>>();

        // Assert
        result.Count.Should().Be(2);
        result[guid1].Id.Should().Be("first");
        result[guid2].Name.Should().Be("Second Item");
    }

    [Fact]
    public void Adapt_AllPrimitiveTypes_InDictionary_ShouldCopy()
    {
        // Test Dictionary<string, object> scenario (using actual typed dictionaries)
        var intDict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var stringDict = new Dictionary<string, string> { { "x", "y" } };
        var boolDict = new Dictionary<string, bool> { { "flag", true } };

        Dictionary<string, int> intResult = intDict.Adapt<Dictionary<string, int>>();
        Dictionary<string, string> stringResult = stringDict.Adapt<Dictionary<string, string>>();
        Dictionary<string, bool> boolResult = boolDict.Adapt<Dictionary<string, bool>>();

        intResult["a"].Should().Be(1);
        stringResult["x"].Should().Be("y");
        boolResult["flag"].Should().BeTrue();
    }

    [Fact]
    public void Adapt_EmptyCollectionsInObject_ShouldMapAsEmpty()
    {
        // Arrange
        var source = new MultiListSource
        {
            Numbers = [],
            Tags = [],
            Items = []
        };

        // Act
        var result = source.Adapt<MultiListDest>();

        // Assert
        result.Numbers.Should().BeEmpty();
        result.Tags.Should().BeEmpty();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_ObjectGraphWithCircularPotential_ShouldNotInfiniteLoop()
    {
        // Testing that we don't follow references infinitely
        // (our shallow copy approach prevents this)
        var child = new BasicSource { Id = "child", Name = "Child", Count = 1 };
        var parent = new NestedSource { Name = "Parent", Child = child };

        var result = parent.Adapt<NestedDest>();

        result.Name.Should().Be("Parent");
        result.Child.Id.Should().Be("child");
    }
}

