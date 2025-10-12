using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class EdgeCaseTests : UnitTest
{
    public EdgeCaseTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_EmptyStrings_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource { Id = "", Name = "", Count = 0 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("");
        result.Name.Should().Be("");
    }

    [Fact]
    public void Adapt_Whitespace_ShouldMapAsIs()
    {
        // Arrange
        var source = new BasicSource { Id = "   ", Name = "\t\n", Count = 0 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("   ");
        result.Name.Should().Be("\t\n");
    }

    [Fact]
    public void Adapt_SpecialCharacters_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource 
        { 
            Id = "!@#$%^&*()", 
            Name = "äöüß€£¥", 
            Count = 42 
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("!@#$%^&*()");
        result.Name.Should().Be("äöüß€£¥");
    }

    [Fact]
    public void Adapt_UnicodeEmojis_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource { Id = "🚀", Name = "🎉🎊", Count = 1 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Should().Be("🚀");
        result.Name.Should().Be("🎉🎊");
    }

    [Fact]
    public void Adapt_VeryLongStrings_ShouldMapCorrectly()
    {
        // Arrange
        var longString = new string('a', 10000);
        var source = new BasicSource { Id = longString, Name = longString, Count = 0 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Length.Should().Be(10000);
        result.Name.Length.Should().Be(10000);
    }

    [Fact]
    public void Adapt_MinMaxValues_Decimal_ShouldMap()
    {
        // Arrange
        var source = new DecimalSource 
        { 
            Price = decimal.MaxValue, 
            Discount = decimal.MinValue 
        };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(decimal.MaxValue);
        result.Discount.Should().Be(decimal.MinValue);
    }

    [Fact]
    public void Adapt_ZeroValues_ShouldMap()
    {
        // Arrange
        var source = new DecimalSource { Price = 0m, Discount = 0m };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(0m);
        result.Discount.Should().Be(0m);
    }

    [Fact]
    public void Adapt_NegativeNumbers_ShouldMap()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "test", Count = -999 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Count.Should().Be(-999);
    }

    [Fact]
    public void Adapt_DefaultGuid_ShouldMap()
    {
        // Arrange
        var source = new GuidSource { Id = default, Name = "default" };

        // Act
        var result = source.Adapt<GuidDest>();

        // Assert
        result.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Adapt_ListOfGuids_AllDefault_ShouldCopy()
    {
        // Arrange
        var source = new List<Guid> { default, default, default };

        // Act
        List<Guid> result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result.Should().AllSatisfy(g => g.Should().Be(Guid.Empty));
    }

    [Fact]
    public void Adapt_VerySmallDecimal_ShouldMap()
    {
        // Arrange
        var source = new DecimalSource { Price = 0.000001m, Discount = 0.000000001m };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(0.000001m);
        result.Discount.Should().Be(0.000000001m);
    }

    [Fact]
    public void Adapt_MultipleAdaptCalls_ShouldCreateIndependentObjects()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "name", Count = 1 };

        // Act
        var result1 = source.Adapt<BasicDest>();
        var result2 = source.Adapt<BasicDest>();
        var result3 = source.Adapt<BasicDest>();

        // Assert
        result1.Should().NotBeSameAs(result2);
        result2.Should().NotBeSameAs(result3);
        result1.Should().NotBeSameAs(result3);
        
        // But all should have same values
        result1.Id.Should().Be("test");
        result2.Id.Should().Be("test");
        result3.Id.Should().Be("test");
    }

    [Fact]
    public void Adapt_ListAfterModification_ShouldNotAffectOriginal()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        List<int> result = source.Adapt();
        result.Add(4);
        result.Add(5);

        // Assert
        source.Count.Should().Be(3);
        result.Count.Should().Be(5);
        source.Should().BeEquivalentTo([1, 2, 3]);
        result.Should().BeEquivalentTo([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Adapt_DictionaryAfterModification_ShouldNotAffectOriginal()
    {
        // Arrange
        var source = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

        // Act
        Dictionary<string, int> result = source.Adapt();
        result["c"] = 3;
        result["b"] = 999;

        // Assert
        source.Count.Should().Be(2);
        result.Count.Should().Be(3);
        source["b"].Should().Be(2);
        result["b"].Should().Be(999);
    }

    [Fact]
    public void Adapt_MixedCasePropertyNames_ShouldMatchExactly()
    {
        // This tests that property matching is case-sensitive
        // Only exact name matches should work
        var source = new BasicSource { Id = "ID", Name = "NAME", Count = 1 };
        var result = source.Adapt<BasicDest>();
        
        result.Id.Should().Be("ID");
        result.Name.Should().Be("NAME");
    }

    [Fact]
    public void Adapt_ListOfMixedContent_ShouldCopyAll()
    {
        // Arrange
        var source = new List<string> 
        { 
            "", 
            "normal", 
            "   ", 
            "🎉", 
            "very long string here with lots of content",
            null
        };

        // Act
        List<string> result = source.Adapt();

        // Assert
        result.Count.Should().Be(6);
        result[0].Should().Be("");
        result[1].Should().Be("normal");
        result[2].Should().Be("   ");
        result[3].Should().Be("🎉");
        result[4].Should().Contain("very long");
        result[5].Should().BeNull();
    }
}

