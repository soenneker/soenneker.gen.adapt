using Soenneker.Tests.Unit;
using System;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class BasicMappingTests : UnitTest
{
    public BasicMappingTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_BasicPropertyMapping_ShouldMapMatchingProperties()
    {
        // Arrange
        var source = new BasicSource
        {
            Id = "test-123",
            Name = "Test Name",
            Count = 42
        };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("test-123");
        result.Name.Should().Be("Test Name");
        result.Count.Should().Be(42);
    }

    [Fact]
    public void Adapt_PartialPropertyMatch_ShouldMapOnlyMatchingProperties()
    {
        // Arrange
        var source = new PartialMatchSource
        {
            Id = "partial-123",
            Name = "Partial Name",
            ExtraField = "Should be ignored",
            Count = 100
        };

        // Act
        var result = source.Adapt<PartialMatchDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("partial-123");
        result.Name.Should().Be("Partial Name");
    }

    [Fact]
    public void Adapt_Guid_ShouldMapCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var source = new GuidSource
        {
            Id = guid,
            Name = "Test Guid"
        };

        // Act
        var result = source.Adapt<GuidDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(guid);
        result.Name.Should().Be("Test Guid");
    }

    [Fact]
    public void Adapt_DateTime_ShouldMapCorrectly()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        var source = new DateTimeSource
        {
            CreatedAt = now,
            UpdatedAt = now.AddDays(1)
        };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().Be(now);
        result.UpdatedAt.Should().Be(now.AddDays(1));
    }

    [Fact]
    public void Adapt_Bool_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BoolSource
        {
            IsActive = true,
            IsDeleted = false
        };

        // Act
        var result = source.Adapt<BoolDest>();

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Adapt_Decimal_ShouldMapCorrectly()
    {
        // Arrange
        var source = new DecimalSource
        {
            Price = 99.99m,
            Discount = 10.5m
        };

        // Act
        var result = source.Adapt<DecimalDest>();

        // Assert
        result.Should().NotBeNull();
        result.Price.Should().Be(99.99m);
        result.Discount.Should().Be(10.5m);
    }

    [Fact]
    public void Adapt_ClassWithMultipleProperties_ShouldMapProperties()
    {
        // Arrange  
        var source = new PersonRecordSource
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30
        };

        // Act
        var result = source.Adapt<PersonRecordDest>();

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Age.Should().Be(30);
    }

    [Fact]
    public void Adapt_Struct_ShouldMapProperties()
    {
        // Arrange
        var source = new PointStructSource
        {
            X = 10,
            Y = 20
        };

        // Act
        var result = source.Adapt<PointStructDest>();

        // Assert
        result.X.Should().Be(10);
        result.Y.Should().Be(20);
    }

    [Fact]
    public void Adapt_ClassWithStructProperty_ShouldMapRecursively()
    {
        // Arrange
        var source = new MixedStructSource
        {
            Location = new PointStructSource { X = 100, Y = 200 },
            Label = "Point A"
        };

        // Act
        var result = source.Adapt<MixedStructDest>();

        // Assert
        result.Should().NotBeNull();
        result.Location.X.Should().Be(100);
        result.Location.Y.Should().Be(200);
        result.Label.Should().Be("Point A");
    }

    [Fact]
    public void Adapt_MultipleInstancesOfSameType_ShouldCreateIndependentObjects()
    {
        // Arrange
        var source1 = new BasicSource { Id = "1", Name = "First", Count = 10 };
        var source2 = new BasicSource { Id = "2", Name = "Second", Count = 20 };

        // Act
        var result1 = source1.Adapt<BasicDest>();
        var result2 = source2.Adapt<BasicDest>();

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().NotBeSameAs(result2);
        result1.Id.Should().Be("1");
        result2.Id.Should().Be("2");
    }

    [Fact]
    public void Adapt_EmptyStrings_ShouldMapEmptyStrings()
    {
        // Arrange
        var source = new BasicSource { Id = "", Name = "", Count = 0 };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("");
        result.Name.Should().Be("");
        result.Count.Should().Be(0);
    }

    [Fact]
    public void Adapt_MinMaxIntValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "test", Count = int.MaxValue };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Count.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Adapt_MinIntValues_ShouldMapCorrectly()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "test", Count = int.MinValue };

        // Act
        var result = source.Adapt<BasicDest>();

        // Assert
        result.Count.Should().Be(int.MinValue);
    }

    [Fact]
    public void Adapt_DefaultDateTime_ShouldMapCorrectly()
    {
        // Arrange
        var source = new DateTimeSource
        {
            CreatedAt = default,
            UpdatedAt = null // nullable DateTime
        };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.CreatedAt.Should().Be(default(DateTime));
        result.UpdatedAt.Should().BeNull(); // nullable property
    }

    [Fact]
    public void Adapt_MaxDateTime_ShouldMapCorrectly()
    {
        // Arrange
        var source = new DateTimeSource
        {
            CreatedAt = DateTime.MaxValue,
            UpdatedAt = DateTime.MinValue
        };

        // Act
        var result = source.Adapt<DateTimeDest>();

        // Assert
        result.CreatedAt.Should().Be(DateTime.MaxValue);
        result.UpdatedAt.Should().Be(DateTime.MinValue);
    }
}

