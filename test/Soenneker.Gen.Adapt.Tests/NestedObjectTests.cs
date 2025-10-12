using Soenneker.Tests.Unit;
using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class NestedObjectTests : UnitTest
{
    public NestedObjectTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_NestedObject_ShouldMapRecursively()
    {
        // Arrange
        var source = new NestedSource
        {
            Name = "Parent",
            Child = new BasicSource
            {
                Id = "child-1",
                Name = "Child Name",
                Count = 100
            }
        };

        // Act
        var result = source.Adapt<NestedDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Parent");
        result.Child.Should().NotBeNull();
        result.Child.Id.Should().Be("child-1");
        result.Child.Name.Should().Be("Child Name");
        result.Child.Count.Should().Be(100);
    }

    [Fact]
    public void Adapt_DeepNesting_ShouldMapRecursively()
    {
        // Arrange
        var source = new DeepNestedSource
        {
            RootName = "Root",
            Level1 = new NestedSource
            {
                Name = "Level1",
                Child = new BasicSource
                {
                    Id = "deep-1",
                    Name = "Deep Child",
                    Count = 999
                }
            }
        };

        // Act
        var result = source.Adapt<DeepNestedDest>();

        // Assert
        result.Should().NotBeNull();
        result.RootName.Should().Be("Root");
        result.Level1.Should().NotBeNull();
        result.Level1.Name.Should().Be("Level1");
        result.Level1.Child.Should().NotBeNull();
        result.Level1.Child.Id.Should().Be("deep-1");
        result.Level1.Child.Name.Should().Be("Deep Child");
        result.Level1.Child.Count.Should().Be(999);
    }

    [Fact]
    public void Adapt_NestedObject_MultipleChildren_ShouldMapAll()
    {
        // Arrange
        var source = new ComplexListSource
        {
            NestedItems =
            [
                new()
                {
                    Name = "Parent1",
                    Child = new BasicSource { Id = "c1", Name = "Child1", Count = 1 }
                },

                new()
                {
                    Name = "Parent2",
                    Child = new BasicSource { Id = "c2", Name = "Child2", Count = 2 }
                },

                new()
                {
                    Name = "Parent3",
                    Child = new BasicSource { Id = "c3", Name = "Child3", Count = 3 }
                }
            ]
        };

        // Act
        var result = source.Adapt<ComplexListDest>();

        // Assert
        result.NestedItems.Count.Should().Be(3);
        result.NestedItems[0].Name.Should().Be("Parent1");
        result.NestedItems[0].Child.Id.Should().Be("c1");
        result.NestedItems[1].Name.Should().Be("Parent2");
        result.NestedItems[1].Child.Id.Should().Be("c2");
        result.NestedItems[2].Name.Should().Be("Parent3");
        result.NestedItems[2].Child.Id.Should().Be("c3");
    }

    [Fact]
    public void Adapt_NestedObjectWithEmptyList_ShouldMap()
    {
        // Arrange
        var source = new ComplexListSource
        {
            NestedItems = []
        };

        // Act
        var result = source.Adapt<ComplexListDest>();

        // Assert
        result.Should().NotBeNull();
        result.NestedItems.Should().NotBeNull();
        result.NestedItems.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_NestedObjectsInDictionary_ShouldMap()
    {
        // Arrange - using Dictionary with nested complex objects
        var source = new Dictionary<string, NestedSource>
        {
            {
                "first",
                new NestedSource
                {
                    Name = "FirstNested",
                    Child = new BasicSource { Id = "1", Name = "One", Count = 1 }
                }
            },
            {
                "second",
                new NestedSource
                {
                    Name = "SecondNested",
                    Child = new BasicSource { Id = "2", Name = "Two", Count = 2 }
                }
            }
        };

        // Act
        Dictionary<string, NestedSource> result = source.Adapt();

        // Assert
        result.Count.Should().Be(2);
        result["first"].Name.Should().Be("FirstNested");
        result["first"].Child.Id.Should().Be("1");
        result["second"].Name.Should().Be("SecondNested");
        result["second"].Child.Id.Should().Be("2");
    }

    [Fact]
    public void Adapt_ComplexNestedStructure_ShouldMapAll()
    {
        // Arrange
        var source = new DeepNestedSource
        {
            RootName = "Root",
            Level1 = new NestedSource
            {
                Name = "Level1Name",
                Child = new BasicSource
                {
                    Id = "child-id",
                    Name = "ChildName",
                    Count = 555
                }
            }
        };

        // Act
        var result = source.Adapt<DeepNestedDest>();

        // Assert
        // Verify entire chain
        result.RootName.Should().Be("Root");
        result.Level1.Should().NotBeNull();
        result.Level1.Name.Should().Be("Level1Name");
        result.Level1.Child.Should().NotBeNull();
        result.Level1.Child.Id.Should().Be("child-id");
        result.Level1.Child.Name.Should().Be("ChildName");
        result.Level1.Child.Count.Should().Be(555);
    }
}

