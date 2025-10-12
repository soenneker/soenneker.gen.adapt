using Soenneker.Tests.Unit;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

[Collection("Collection")]
public sealed class IEnumerableTests : UnitTest
{
    public IEnumerableTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_IEnumerable_Int_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Range(1, 5);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Adapt_IEnumerable_String_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<string> source = new[] { "a", "b", "c" }.Where(x => x != "d");

        // Act
        IEnumerable<string> result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [Fact]
    public void Adapt_IEnumerable_Empty_ShouldReturnEmpty()
    {
        // Arrange
        IEnumerable<int> source = [];

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_IEnumerable_FromLinq_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Range(1, 10)
            .Where(x => x % 2 == 0)
            .Select(x => x * 2);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo([4, 8, 12, 16, 20]);
    }

    [Fact]
    public void Adapt_IEnumerable_ComplexLinqQuery_ShouldMaterialize()
    {
        // Arrange
        var data = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        IEnumerable<int> source = data
            .Where(x => x > 3)
            .OrderByDescending(x => x)
            .Take(5);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo([10, 9, 8, 7, 6]);
    }

    [Fact]
    public void Adapt_IEnumerable_SelectMany_ShouldMaterialize()
    {
        // Arrange
        var data = new[] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5, 6 } };
        IEnumerable<int> source = data.SelectMany(x => x);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Adapt_IEnumerable_Distinct_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = new[] { 1, 2, 2, 3, 3, 3, 4 }.Distinct();

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo([1, 2, 3, 4]);
    }

    [Fact]
    public void Adapt_IEnumerable_WithNull_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<string?> source = ["a", null, "c"];

        // Act
        IEnumerable<string?> result = source.Adapt();

        // Assert
        result.Count().Should().Be(3);
        result.ElementAt(0).Should().Be("a");
        result.ElementAt(1).Should().BeNull();
        result.ElementAt(2).Should().Be("c");
    }

    [Fact]
    public void Adapt_IEnumerable_SingleElement_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<string> source = ["only"];

        // Act
        IEnumerable<string> result = source.Adapt();

        // Assert
        result.Count().Should().Be(1);
        result.First().Should().Be("only");
    }

    [Fact]
    public void Adapt_IEnumerable_GroupBy_ShouldMaterialize()
    {
        // Arrange
        var data = new[] { 1, 2, 3, 4, 5, 6 };
        IEnumerable<int> source = data.GroupBy(x => x % 2).SelectMany(g => g);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(6);
    }

    [Fact]
    public void Adapt_IEnumerable_Zip_ShouldMaterialize()
    {
        // Arrange
        var first = new[] { 1, 2, 3 };
        var second = new[] { 10, 20, 30 };
        IEnumerable<int> source = first.Zip(second, (a, b) => a + b);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo([11, 22, 33]);
    }

    [Fact]
    public void Adapt_IEnumerable_Concat_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = new[] { 1, 2, 3 }.Concat([4, 5, 6]);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Adapt_IEnumerable_Skip_Take_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Range(1, 20).Skip(5).Take(10);

        // Act
        IEnumerable<int> result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo([6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }
}

