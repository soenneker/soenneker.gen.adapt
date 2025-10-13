using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionComplexTypesTests : UnitTest
{
    public ReflectionComplexTypesTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_SimpleNestedObject_ShouldMapTopLevelProperties()
    {
        // Arrange
        var source = new NestedObjectSource
        {
            Name = "Parent",
            Inner = new InnerObjectSource
            {
                Id = 123,
                Value = "Inner Value",
                Timestamp = DateTime.Now
            }
        };

        // Act
        var result = source.AdaptViaReflection<NestedObjectDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        // Note: Inner won't be deeply copied since InnerObjectSource is not assignable to InnerObjectDest
        // Reflection adapter only copies assignable properties directly
    }

    [Fact]
    public void AdaptViaReflection_NestedObject_WithNullInner_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NestedObjectSource
        {
            Name = "Parent Only",
            Inner = null
        };

        // Act
        var result = source.AdaptViaReflection<NestedObjectDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Inner.Should().BeNull();
    }

    [Fact]
    public void AdaptViaReflection_DeepNestedObject_ShouldMapTopLevelProperties()
    {
        // Arrange
        var source = new DeepNestedSource
        {
            Name = "Root",
            Level1 = new Level1Source
            {
                Id = 1,
                Level2 = new Level2Source
                {
                    Value = "Level 2",
                    Level3 = new Level3Source
                    {
                        Number = 3.14,
                        Flag = true
                    }
                }
            }
        };

        // Act
        var result = source.AdaptViaReflection<DeepNestedDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        // Deep nested properties require type-compatible assignments
    }

    [Fact]
    public void AdaptViaReflection_ComplexWithCollections_TopLevelProperties_ShouldMap()
    {
        // Arrange
        var source = new ComplexWithCollectionsSource
        {
            Name = "Complex Object",
            Tags = new List<string> { "tag1", "tag2", "tag3" },
            Counts = new Dictionary<string, int>
            {
                { "apples", 5 },
                { "oranges", 10 }
            },
            Children = new List<ChildItemSource>
            {
                new ChildItemSource { Id = 1, Name = "Child1", CreatedAt = DateTime.Now },
                new ChildItemSource { Id = 2, Name = "Child2", CreatedAt = DateTime.Now.AddDays(-1) }
            }
        };

        // Act
        var result = source.AdaptViaReflection<ComplexWithCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Tags.Should().NotBeNull();
        result.Tags.Should().Equal(source.Tags);
        result.Counts.Should().NotBeNull();
        result.Counts.Count.Should().Be(2);
        // Children should now be recursively adapted!
        result.Children.Should().NotBeNull();
        result.Children.Count.Should().Be(2);
        result.Children[0].Id.Should().Be(1);
        result.Children[0].Name.Should().Be("Child1");
        result.Children[1].Id.Should().Be(2);
        result.Children[1].Name.Should().Be("Child2");
    }

    [Fact]
    public void AdaptViaReflection_ComplexWithCollections_EmptyCollections_ShouldMap()
    {
        // Arrange
        var source = new ComplexWithCollectionsSource
        {
            Name = "Empty Collections",
            Tags = new List<string>(),
            Counts = new Dictionary<string, int>(),
            Children = new List<ChildItemSource>()
        };

        // Act
        var result = source.AdaptViaReflection<ComplexWithCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Tags.Should().BeEmpty(); // List<string> is directly assignable
        result.Counts.Should().BeEmpty(); // Dictionary<string, int> is directly assignable
        // Now Children WILL be adapted recursively!
        result.Children.Should().BeEmpty();
    }

    [Fact]
    public void AdaptViaReflection_ComplexWithCollections_NullCollections_ShouldMap()
    {
        // Arrange
        var source = new ComplexWithCollectionsSource
        {
            Name = "Null Collections",
            Tags = null,
            Counts = null,
            Children = null
        };

        // Act
        var result = source.AdaptViaReflection<ComplexWithCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Tags.Should().BeNull();
        result.Counts.Should().BeNull();
        result.Children.Should().BeNull();
    }

    [Fact]
    public void AdaptViaReflection_CircularReference_ShouldMapAvailableProperties()
    {
        // Arrange
        var source = new CircularReferenceSource
        {
            Id = 42,
            Name = "Circular Test"
        };

        // Act
        var result = source.AdaptViaReflection<CircularReferenceDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
    }

    [Fact]
    public void AdaptViaReflection_InnerObject_Directly_ShouldMap()
    {
        // Arrange
        var source = new InnerObjectSource
        {
            Id = 999,
            Value = "Direct Inner",
            Timestamp = new DateTime(2024, 6, 15, 10, 30, 0)
        };

        // Act
        var result = source.AdaptViaReflection<InnerObjectDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Value.Should().Be(source.Value);
        result.Timestamp.Should().Be(source.Timestamp);
    }

    [Fact]
    public void AdaptViaReflection_Level1_Directly_ShouldMap()
    {
        // Arrange
        var source = new Level1Source
        {
            Id = 111
        };

        // Act
        var result = source.AdaptViaReflection<Level1Dest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
    }

    [Fact]
    public void AdaptViaReflection_Level3_Directly_ShouldMap()
    {
        // Arrange
        var source = new Level3Source
        {
            Number = 9.81,
            Flag = false
        };

        // Act
        var result = source.AdaptViaReflection<Level3Dest>();

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().Be(source.Number);
        result.Flag.Should().Be(source.Flag);
    }

    [Fact]
    public void AdaptViaReflection_ChildItem_Directly_ShouldMap()
    {
        // Arrange
        var source = new ChildItemSource
        {
            Id = 777,
            Name = "Child Test",
            CreatedAt = new DateTime(2024, 3, 15)
        };

        // Act
        var result = source.AdaptViaReflection<ChildItemDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        result.CreatedAt.Should().Be(source.CreatedAt);
    }

    [Fact]
    public void AdaptViaReflection_ComplexWithLargeCollections_ShouldMap()
    {
        // Arrange
        var source = new ComplexWithCollectionsSource
        {
            Name = "Large Collections",
            Tags = new List<string>()
        };

        for (int i = 0; i < 1000; i++)
        {
            source.Tags.Add($"Tag{i}");
        }

        // Act
        var result = source.AdaptViaReflection<ComplexWithCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Tags.Count.Should().Be(1000);
        result.Tags[0].Should().Be("Tag0");
        result.Tags[999].Should().Be("Tag999");
    }

    [Fact]
    public void AdaptViaReflection_ComplexWithDeeplyNestedDictionary_ShouldMap()
    {
        // Arrange
        var source = new ComplexWithCollectionsSource
        {
            Name = "Nested Dict Test",
            Counts = new Dictionary<string, int>()
        };

        for (int i = 0; i < 100; i++)
        {
            source.Counts[$"key{i}"] = i * 10;
        }

        // Act
        var result = source.AdaptViaReflection<ComplexWithCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Counts.Count.Should().Be(100);
        result.Counts["key50"].Should().Be(500);
    }
}

