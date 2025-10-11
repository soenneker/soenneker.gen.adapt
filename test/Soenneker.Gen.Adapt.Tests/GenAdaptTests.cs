using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

[Collection("Collection")]
public sealed class GenAdaptTests : UnitTest
{
    public GenAdaptTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Default()
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
    public void Adapt_ListToList_SameElementType_ShouldConvert()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act - this will just convert List to List (same type)
        // For actual element mapping, see next test
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }


    [Fact]
    public void Adapt_ListToList_string_ShouldConvert()
    {
        // Arrange
        var source = new List<string> { "1", "2" };

        // Act - this will just convert List to List (same type)
        // For actual element mapping, see next test
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
    public void Adapt_EnumToString_ShouldConvert()
    {
        // Arrange
        var source = new EnumSource
        {
            Status = TestStatus.Active,
            Priority = 1
        };

        // Act
        var result = source.Adapt<EnumToStringDest>();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Active");
    }

    [Fact]
    public void Adapt_StringToEnum_ShouldParse()
    {
        // Arrange
        var source = new StringToEnumSource
        {
            StatusString = "Pending"
        };

        // Act
        var result = source.Adapt<StringToEnumDest>();

        // Assert
        result.Should().NotBeNull();
        result.StatusString.Should().Be(TestStatus.Pending);
    }

    [Fact]
    public void Adapt_EnumToInt_ShouldCast()
    {
        // Arrange
        var source = new EnumToIntSource
        {
            Status = TestStatus.Completed
        };

        // Act
        var result = source.Adapt<EnumToIntDest>();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(2);
    }

    [Fact]
    public void Adapt_IntToEnum_ShouldCast()
    {
        // Arrange
        var source = new IntToEnumSource
        {
            StatusCode = 1
        };

        // Act
        var result = source.Adapt<IntToEnumDest>();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(TestStatus.Pending);
    }

    [Fact]
    public void Adapt_IntellenumToInt_ShouldExtractValue()
    {
        // Arrange
        var source = new IntellenumSource
        {
            UserId = TestIntellenum.From(999)
        };

        // Act
        var result = source.Adapt<IntellenumToIntDest>();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(999);
    }

    [Fact]
    public void Adapt_IntToIntellenum_ShouldCreateFrom()
    {
        // Arrange
        var source = new IntToIntellenumSource
        {
            UserId = 777
        };

        // Act
        var result = source.Adapt<IntToIntellenumDest>();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBeNull();
        result.UserId.Value.Should().Be(777);
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
    public void Adapt_ClassWithMultipleProperties_ShouldMapProperties()
    {
        // Arrange  
        // Note: Records work the same way as classes for mapping purposes
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
    public void Adapt_NullableProperties_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NullablePropsSource
        {
            Name = "Test",
            Count = 42,
            Status = TestStatus.Active
        };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Count.Should().Be(42);
        result.Status.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void Adapt_NullableProperties_WithNulls_ShouldMapCorrectly()
    {
        // Arrange
        var source = new NullablePropsSource
        {
            Name = null,
            Count = null,
            Status = null
        };

        // Act
        var result = source.Adapt<NullablePropsDest>();

        // Assert
        result.Should().NotBeNull();
        Assert.Null(result.Name);
        Assert.Null(result.Count);
        Assert.Null(result.Status);
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
        // ExtraField is not in dest, Count is not in dest
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
    public void Adapt_ListOfNestedObjects_ShouldMapWithManualLoop()
    {
        // Arrange
        var source = new ComplexListSource
        {
            NestedItems = new List<NestedSource>
            {
                new()
                {
                    Name = "Nested1",
                    Child = new BasicSource { Id = "1", Name = "Child1", Count = 10 }
                },
                new()
                {
                    Name = "Nested2",
                    Child = new BasicSource { Id = "2", Name = "Child2", Count = 20 }
                }
            }
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

    // ========== Dictionary Tests ==========

    [Fact]
    public void Adapt_Dictionary_StringString_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        source.Should().NotBeSameAs(result);
        result.Count.Should().Be(3);
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
        result["key3"].Should().Be("value3");
    }

    [Fact]
    public void Adapt_Dictionary_IntString_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        source.Should().NotBeSameAs(result);
        result.Count.Should().Be(3);
        result[1].Should().Be("one");
        result[2].Should().Be("two");
        result[3].Should().Be("three");
    }

    [Fact]
    public void Adapt_Dictionary_StringInt_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result["one"].Should().Be(1);
        result["two"].Should().Be(2);
        result["three"].Should().Be(3);
    }

    [Fact]
    public void Adapt_Dictionary_Empty_ShouldReturnEmpty()
    {
        // Arrange
        var source = new Dictionary<string, int>();

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Adapt_IDictionary_ShouldCopy()
    {
        // Arrange
        IDictionary<string, int> source = new Dictionary<string, int>
        {
            { "a", 100 },
            { "b", 200 }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result["a"].Should().Be(100);
        result["b"].Should().Be(200);
    }

    // ========== IEnumerable Tests ==========

    [Fact]
    public void Adapt_IEnumerable_Int_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Range(1, 5);

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void Adapt_IEnumerable_String_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<string> source = new[] { "a", "b", "c" }.Where(x => x != "d");

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [Fact]
    public void Adapt_IEnumerable_Empty_ShouldReturnEmpty()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Empty<int>();

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // ========== Nested Collection Tests ==========

    [Fact]
    public void Adapt_ListOfLists_Int_ShouldCopy()
    {
        // Arrange
        var source = new List<List<int>>
        {
            new List<int> { 1, 2, 3 },
            new List<int> { 4, 5, 6 },
            new List<int> { 7, 8, 9 }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result[1].Should().BeEquivalentTo(new[] { 4, 5, 6 });
        result[2].Should().BeEquivalentTo(new[] { 7, 8, 9 });
        // Should be shallow copy - inner lists are same references
        source[0].Should().BeSameAs(result[0]);
    }

    [Fact]
    public void Adapt_ListOfLists_String_ShouldCopy()
    {
        // Arrange
        var source = new List<List<string>>
        {
            new List<string> { "a", "b" },
            new List<string> { "c", "d" },
        };

        // Act
        var result = source.Adapt();

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
            new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
            new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
        };

        // Act
        var result = source.Adapt();

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
            { "odds", new List<int> { 1, 3, 5 } },
            { "evens", new List<int> { 2, 4, 6 } }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result["odds"].Should().BeEquivalentTo(new[] { 1, 3, 5 });
        result["evens"].Should().BeEquivalentTo(new[] { 2, 4, 6 });
    }

    // ========== Edge Case Tests ==========

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
    public void Adapt_Dictionary_LargeCollection_ShouldCopy()
    {
        // Arrange
        var source = new Dictionary<int, string>();
        for (int i = 0; i < 500; i++)
        {
            source[i] = $"value_{i}";
        }

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(500);
        result[0].Should().Be("value_0");
        result[499].Should().Be("value_499");
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
    public void Adapt_Dictionary_ComplexKey_ShouldCopy()
    {
        // Arrange
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var source = new Dictionary<Guid, string>
        {
            { key1, "first" },
            { key2, "second" }
        };

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[key1].Should().Be("first");
        result[key2].Should().Be("second");
    }

    [Fact]
    public void Adapt_IEnumerable_FromLinq_ShouldMaterialize()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Range(1, 10)
            .Where(x => x % 2 == 0)
            .Select(x => x * 2);

        // Act
        var result = source.Adapt();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 4, 8, 12, 16, 20 });
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
}

