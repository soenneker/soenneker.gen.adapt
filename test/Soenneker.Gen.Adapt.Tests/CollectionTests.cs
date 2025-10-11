using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class CollectionTests : UnitTest
{
    public CollectionTests(ITestOutputHelper output) : base(output)
    {
    }

    // ========== List Tests ==========

    [Fact]
    public void Adapt_ListToList_SameElementType_Int_ShouldConvert()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void Adapt_ListToList_SameElementType_String_ShouldConvert()
    {
        // Arrange
        var source = new List<string> { "1", "2" };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { "1", "2" });
    }

    [Fact]
    public void Adapt_ListToList_DifferentElementTypes_ShouldMapEachElement()
    {
        // Arrange
        var sources = new CollectionSource
        {
            Items = new List<BasicSource>
            {
                new() { Id = "1", Name = "First", Count = 10 },
                new() { Id = "2", Name = "Second", Count = 20 }
            }
        };

        // Act
        var result = sources.Adapt<CollectionDest>();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Items.Count.Should().Be(2);
        result.Items[0].Id.Should().Be("1");
        result.Items[0].Name.Should().Be("First");
        result.Items[1].Id.Should().Be("2");
        result.Items[1].Name.Should().Be("Second");
    }

    [Fact]
    public void Adapt_EmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var source = new List<int>();

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_List_WithDuplicates_ShouldCopy()
    {
        // Arrange
        var source = new List<int> { 1, 2, 2, 3, 3, 3 };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(6);
        result.Should().BeEquivalentTo(new[] { 1, 2, 2, 3, 3, 3 });
    }

    [Fact]
    public void Adapt_List_LargeCollection_ShouldCopy()
    {
        // Arrange
        var source = new List<int>(Enumerable.Range(1, 1000));

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1000);
        result[0].Should().Be(1);
        result[999].Should().Be(1000);
    }

    [Fact]
    public void Adapt_List_Guid_ShouldCopy()
    {
        // Arrange
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var source = new List<Guid> { guid1, guid2 };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].Should().Be(guid1);
        result[1].Should().Be(guid2);
    }

    [Fact]
    public void Adapt_List_DateTime_ShouldCopy()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var tomorrow = now.AddDays(1);
        var source = new List<DateTime> { now, tomorrow };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].Should().Be(now);
        result[1].Should().Be(tomorrow);
    }

    [Fact]
    public void Adapt_List_Decimal_ShouldCopy()
    {
        // Arrange
        var source = new List<decimal> { 1.5m, 2.75m, 3.99m };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1.5m, 2.75m, 3.99m });
    }

    [Fact]
    public void Adapt_List_Long_ShouldCopy()
    {
        // Arrange
        var source = new List<long> { 1000000000L, 2000000000L, 3000000000L };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1000000000L, 2000000000L, 3000000000L });
    }

    [Fact]
    public void Adapt_List_Bool_ShouldCopy()
    {
        // Arrange
        var source = new List<bool> { true, false, true, true };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { true, false, true, true });
    }

    [Fact]
    public void Adapt_List_MaintainsOrder_ShouldCopy()
    {
        // Arrange
        var source = new List<string> { "z", "a", "m", "b", "y" };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { "z", "a", "m", "b", "y" });
        source.Should().NotBeSameAs(result);
    }

    [Fact]
    public void Adapt_MultipleLists_ShouldMapAllCorrectly()
    {
        // Arrange
        var source = new MultiListSource
        {
            Numbers = new List<int> { 1, 2, 3 },
            Tags = new List<string> { "a", "b", "c" },
            Items = new List<BasicSource>
            {
                new() { Id = "1", Name = "First", Count = 10 }
            }
        };

        // Act
        var result = source.Adapt<MultiListDest>();

        // Assert
        result.Should().NotBeNull();
        result.Numbers.Count.Should().Be(3);
        result.Numbers.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result.Tags.Should().BeEquivalentTo(new[] { "a", "b", "c" });
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be("1");
    }

    [Fact]
    public void Adapt_SingleElementList_ShouldCopy()
    {
        // Arrange
        var source = new List<string> { "only" };

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(1);
        result[0].Should().Be("only");
    }

    [Fact]
    public void Adapt_ListWithNullElements_ShouldCopyNulls()
    {
        // Arrange
        var source = new List<string> { "a", null, "c" };

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result[0].Should().Be("a");
        result[1].Should().BeNull();
        result[2].Should().Be("c");
    }

    [Fact]
    public void Adapt_List_Double_ShouldCopy()
    {
        // Arrange
        var source = new List<double> { 1.1, 2.2, 3.3 };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo(new[] { 1.1, 2.2, 3.3 });
    }

    [Fact]
    public void Adapt_List_Float_ShouldCopy()
    {
        // Arrange
        var source = new List<float> { 1.1f, 2.2f, 3.3f };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo(new[] { 1.1f, 2.2f, 3.3f });
    }

    [Fact]
    public void Adapt_List_Byte_ShouldCopy()
    {
        // Arrange
        var source = new List<byte> { 0, 128, 255 };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo(new byte[] { 0, 128, 255 });
    }

    [Fact]
    public void Adapt_List_Short_ShouldCopy()
    {
        // Arrange
        var source = new List<short> { -100, 0, 100 };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo(new short[] { -100, 0, 100 });
    }

    [Fact]
    public void Adapt_List_UInt_ShouldCopy()
    {
        // Arrange
        var source = new List<uint> { 0, 100, uint.MaxValue };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo(new uint[] { 0, 100, uint.MaxValue });
    }

    [Fact]
    public void Adapt_List_Char_ShouldCopy()
    {
        // Arrange
        var source = new List<char> { 'a', 'b', 'c', 'Z' };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEquivalentTo(new[] { 'a', 'b', 'c', 'Z' });
    }
}

