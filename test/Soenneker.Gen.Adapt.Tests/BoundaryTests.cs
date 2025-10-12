using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class BoundaryTests : UnitTest
{
    public BoundaryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_Int32_MinValue_ShouldMap()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "test", Count = int.MinValue };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Count.Should().Be(int.MinValue);
        result.Count.Should().Be(-2147483648);
    }

    [Fact]
    public void Adapt_Int32_MaxValue_ShouldMap()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "test", Count = int.MaxValue };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Count.Should().Be(int.MaxValue);
        result.Count.Should().Be(2147483647);
    }

    [Fact]
    public void Adapt_DateTime_MinValue_ShouldMap()
    {
        // Arrange
        var source = new DateTimeSource { CreatedAt = DateTime.MinValue, UpdatedAt = null };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.CreatedAt.Should().Be(DateTime.MinValue);
        result.CreatedAt.Year.Should().Be(1);
    }

    [Fact]
    public void Adapt_DateTime_MaxValue_ShouldMap()
    {
        // Arrange
        var source = new DateTimeSource { CreatedAt = DateTime.MaxValue, UpdatedAt = DateTime.MaxValue };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.CreatedAt.Should().Be(DateTime.MaxValue);
        result.CreatedAt.Year.Should().Be(9999);
    }

    [Fact]
    public void Adapt_DateTimeWithMilliseconds_ShouldPreserve()
    {
        // Arrange
        var source = new DateTimeSource 
        { 
            CreatedAt = new DateTime(2023, 6, 15, 14, 30, 25, 123),
            UpdatedAt = new DateTime(2023, 6, 15, 14, 30, 25, 456)
        };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.CreatedAt.Millisecond.Should().Be(123);
        result.CreatedAt.Should().Be(new DateTime(2023, 6, 15, 14, 30, 25, 123));
    }

    [Fact]
    public void Adapt_DateTimeWithTicks_ShouldPreserve()
    {
        // Arrange
        var ticks = 637915644251234567L;
        var source = new DateTimeSource 
        { 
            CreatedAt = new DateTime(ticks),
            UpdatedAt = null
        };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.CreatedAt.Ticks.Should().Be(ticks);
    }

    [Fact]
    public void Adapt_NullableDateTime_Min_ShouldMap()
    {
        // Arrange
        var source = new List<DateTime?> { DateTime.MinValue, null, DateTime.MaxValue };

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result[0].Should().Be(DateTime.MinValue);
        result[1].Should().BeNull();
        result[2].Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void Adapt_Guid_EmptyAndNewGuid_ShouldMap()
    {
        // Arrange
        var guid1 = Guid.Empty;
        var guid2 = Guid.NewGuid();
        var guid3 = new Guid("00000000-0000-0000-0000-000000000001");
        var guid4 = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

        var source1 = new GuidSource { Id = guid1, Name = "empty" };
        var source2 = new GuidSource { Id = guid2, Name = "random" };
        var source3 = new GuidSource { Id = guid3, Name = "min" };
        var source4 = new GuidSource { Id = guid4, Name = "max" };

        // Act
        var result1 = source1.Adapt<GuidDest>();
        var result2 = source2.Adapt<GuidDest>();
        var result3 = source3.Adapt<GuidDest>();
        var result4 = source4.Adapt<GuidDest>();

        // Assert
        result1.Id.Should().Be(guid1);
        result2.Id.Should().Be(guid2);
        result3.Id.Should().Be(guid3);
        result4.Id.Should().Be(guid4);
    }

    [Fact]
    public void Adapt_GuidWithAllSameDigits_ShouldMap()
    {
        // Arrange
        var guid1 = new Guid("00000000-0000-0000-0000-000000000000");
        var guid2 = new Guid("11111111-1111-1111-1111-111111111111");
        var guid3 = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var source1 = new GuidSource { Id = guid1, Name = "zeros" };
        var source2 = new GuidSource { Id = guid2, Name = "ones" };
        var source3 = new GuidSource { Id = guid3, Name = "as" };

        // Act
        var result1 = source1.Adapt<GuidDest>();
        var result2 = source2.Adapt<GuidDest>();
        var result3 = source3.Adapt<GuidDest>();

        // Assert
        result1.Id.Should().Be(guid1);
        result2.Id.Should().Be(guid2);
        result3.Id.Should().Be(guid3);
    }

    [Fact]
    public void Adapt_NullableGuid_Min_ShouldMap()
    {
        // Arrange
        var source = new List<Guid?> { Guid.Empty, null, Guid.NewGuid() };

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result[0].Should().Be(Guid.Empty);
        result[1].Should().BeNull();
        result[2].Should().NotBeNull();
    }

    [Fact]
    public void Adapt_DictionaryWithGuidKeys_ExtremeValues_ShouldMap()
    {
        // Arrange
        var key1 = Guid.Empty;
        var key2 = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var key3 = new Guid("00000000-0000-0000-0000-000000000001");
        
        var source = new Dictionary<Guid, string>
        {
            { key1, "empty" },
            { key2, "max" },
            { key3, "min" }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(3);
        result[key1].Should().Be("empty");
        result[key2].Should().Be("max");
        result[key3].Should().Be("min");
    }

    [Fact]
    public void Adapt_Decimal_MinMaxValues_ShouldMap()
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
    public void Adapt_Decimal_FullPrecision_ShouldMap()
    {
        // Arrange
        var source = new DecimalSource 
        { 
            Price = 79228162514264337593543950335m,
            Discount = -79228162514264337593543950335m
        };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(79228162514264337593543950335m);
        result.Discount.Should().Be(-79228162514264337593543950335m);
    }

    [Fact]
    public void Adapt_Decimal_ManyDecimalPlaces_ShouldPreserve()
    {
        // Arrange
        var source = new DecimalSource 
        { 
            Price = 0.0000000000000000000000000001m,
            Discount = 1.2345678901234567890123456789m
        };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(0.0000000000000000000000000001m);
        result.Discount.Should().Be(1.2345678901234567890123456789m);
    }

    [Fact]
    public void Adapt_DecimalWithTrailingZeros_ShouldPreserve()
    {
        // Arrange
        var source = new DecimalSource 
        { 
            Price = 1.0m,
            Discount = 10.00m
        };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(1.0m);
        result.Discount.Should().Be(10.00m);
    }

    [Fact]
    public void Adapt_DecimalScientificNotation_ShouldMap()
    {
        // Arrange
        var source = new DecimalSource 
        { 
            Price = 1.23E-10m,
            Discount = 4.56E+20m
        };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Price.Should().Be(0.000000000123m);
        result.Discount.Should().Be(456000000000000000000m);
    }

    [Fact]
    public void Adapt_NumbersAtBoundaries_ShouldMap()
    {
        // Arrange
        var sources = new List<BasicSource>
        {
            new BasicSource { Id = "test", Name = "min", Count = int.MinValue },
            new BasicSource { Id = "test", Name = "min+1", Count = int.MinValue + 1 },
            new BasicSource { Id = "test", Name = "neg1", Count = -1 },
            new BasicSource { Id = "test", Name = "zero", Count = 0 },
            new BasicSource { Id = "test", Name = "pos1", Count = 1 },
            new BasicSource { Id = "test", Name = "max-1", Count = int.MaxValue - 1 },
            new BasicSource { Id = "test", Name = "max", Count = int.MaxValue }
        };

        // Act
        var results = sources.Select(s => s.Adapt<BasicDest>()).ToList();

        // Assert
        results[0].Count.Should().Be(int.MinValue);
        results[1].Count.Should().Be(int.MinValue + 1);
        results[2].Count.Should().Be(-1);
        results[3].Count.Should().Be(0);
        results[4].Count.Should().Be(1);
        results[5].Count.Should().Be(int.MaxValue - 1);
        results[6].Count.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Adapt_ListOfNegativeNumbers_ShouldMap()
    {
        // Arrange
        var source = new List<int> { -1, -2, -3, -100, -1000, int.MinValue };

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(6);
        result.Should().AllSatisfy(x => x.Should().BeLessThan(0));
        result[5].Should().Be(int.MinValue);
    }

    [Fact]
    public void Adapt_HugeList_10000Items_ShouldMapAll()
    {
        // Arrange
        var source = Enumerable.Range(1, 10000).ToList();

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(10000);
        result[0].Should().Be(1);
        result[9999].Should().Be(10000);
        result.Sum().Should().Be(50005000);
    }

    [Fact]
    public void Adapt_HugeDictionary_10000Items_ShouldMapAll()
    {
        // Arrange
        var source = new Dictionary<string, int>();
        for (int i = 0; i < 10000; i++)
        {
            source[$"key_{i}"] = i;
        }

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(10000);
        result["key_0"].Should().Be(0);
        result["key_9999"].Should().Be(9999);
    }

    [Fact]
    public void Adapt_VeryLongString_1MB_ShouldMap()
    {
        // Arrange
        var megabyteString = new string('X', 1024 * 1024);
        var source = new BasicSource { Id = "test", Name = megabyteString, Count = 1 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Name.Length.Should().Be(1024 * 1024);
        result.Name.Should().Be(megabyteString);
    }

    [Fact]
    public void Adapt_VeryLongPropertyValues_ShouldNotCorrupt()
    {
        // Arrange
        var longId = new string('A', 100000);
        var longName = new string('B', 100000);
        var source = new BasicSource { Id = longId, Name = longName, Count = 42 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Id.Length.Should().Be(100000);
        result.Name.Length.Should().Be(100000);
        result.Id.Should().Be(longId);
        result.Name.Should().Be(longName);
    }

    [Fact]
    public void Adapt_IntToEnum_OutOfRange_ShouldStillCast()
    {
        // Arrange
        var source = new IntToEnumSource { StatusCode = 99999 };

        // Act
        var result = source.Adapt<IntToEnumDest>();

        // Assert
        result.StatusCode.Should().Be((TestStatus)99999);
    }

    [Fact]
    public void Adapt_IntToEnum_NegativeValue_ShouldCast()
    {
        // Arrange
        var source = new IntToEnumSource { StatusCode = -1 };

        // Act
        var result = source.Adapt<IntToEnumDest>();

        // Assert
        result.StatusCode.Should().Be((TestStatus)(-1));
    }

    [Fact]
    public void Adapt_EmptyGuidList_ShouldMapToEmptyList()
    {
        // Arrange
        var source = new List<Guid>();

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEmpty();
        result.Should().NotBeSameAs(source);
    }

    [Fact]
    public void Adapt_EmptyDateTimeList_ShouldMapToEmptyList()
    {
        // Arrange
        var source = new List<DateTime>();

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEmpty();
        result.Should().NotBeSameAs(source);
    }

    [Fact]
    public void Adapt_EmptyDecimalList_ShouldMapToEmptyList()
    {
        // Arrange
        var source = new List<decimal>();

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().BeEmpty();
        result.Should().NotBeSameAs(source);
    }
}

