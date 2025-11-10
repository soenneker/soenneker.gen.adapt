using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Collections;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionCollectionsTests : UnitTest
{
    public ReflectionCollectionsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_ListCollections_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ListCollectionsSource
        {
            IntList =
            [
                1,
                2,
                3,
                4,
                5
            ],
            StringList =
            [
                "one",
                "two",
                "three"
            ],
            DoubleList =
            [
                1.1,
                2.2,
                3.3
            ],
            NestedIntList =
            [
                [
                    1,
                    2
                ],

                [
                    3,
                    4
                ],

                [
                    5,
                    6
                ]
            ]
        };

        // Act
        var result = source.AdaptViaReflection<ListCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntList.Should().NotBeNull();
        result.IntList.Should().Equal(source.IntList);
        result.StringList.Should().Equal(source.StringList);
        result.DoubleList.Should().Equal(source.DoubleList);
        result.NestedIntList.Should().NotBeNull();
        result.NestedIntList.Count.Should().Be(3);
    }

    [Fact]
    public void AdaptViaReflection_EmptyLists_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ListCollectionsSource
        {
            IntList = [],
            StringList = [],
            DoubleList = [],
            NestedIntList = []
        };

        // Act
        var result = source.AdaptViaReflection<ListCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntList.Should().BeEmpty();
        result.StringList.Should().BeEmpty();
        result.DoubleList.Should().BeEmpty();
        result.NestedIntList.Should().BeEmpty();
    }

    [Fact]
    public void AdaptViaReflection_LargeLists_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ListCollectionsSource
        {
            IntList = Enumerable.Range(1, 10000).ToList(),
            StringList = Enumerable.Range(1, 5000).Select(i => $"Item{i}").ToList()
        };

        // Act
        var result = source.AdaptViaReflection<ListCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntList.Count.Should().Be(10000);
        result.StringList.Count.Should().Be(5000);
        result.IntList.First().Should().Be(1);
        result.IntList.Last().Should().Be(10000);
    }

    [Fact]
    public void AdaptViaReflection_ArrayCollections_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ArrayCollectionsSource
        {
            IntArray = [1, 2, 3, 4, 5],
            StringArray = ["alpha", "beta", "gamma"],
            DoubleArray = [1.1, 2.2, 3.3],
            JaggedArray =
            [
                [1, 2],
                [3, 4, 5],
                [6]
            ],
            MultiDimensionalArray = new[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } }
        };

        // Act
        var result = source.AdaptViaReflection<ArrayCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().Equal(source.IntArray);
        result.StringArray.Should().Equal(source.StringArray);
        result.DoubleArray.Should().Equal(source.DoubleArray);
        result.JaggedArray.Should().NotBeNull();
        result.JaggedArray.Length.Should().Be(3);
        result.MultiDimensionalArray.Should().NotBeNull();
        result.MultiDimensionalArray.GetLength(0).Should().Be(3);
        result.MultiDimensionalArray.GetLength(1).Should().Be(2);
    }

    [Fact]
    public void AdaptViaReflection_EmptyArrays_ShouldMapCorrectly()
    {
        // Arrange
        var source = new ArrayCollectionsSource
        {
            IntArray = [],
            StringArray = [],
            DoubleArray = []
        };

        // Act
        var result = source.AdaptViaReflection<ArrayCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntArray.Should().BeEmpty();
        result.StringArray.Should().BeEmpty();
        result.DoubleArray.Should().BeEmpty();
    }

    [Fact]
    public void AdaptViaReflection_DictionaryCollections_ShouldMapCorrectly()
    {
        // Arrange
        var source = new DictionaryCollectionsSource
        {
            StringIntDict = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            },
            IntStringDict = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" },
                { 3, "three" }
            },
            StringListDict = new Dictionary<string, List<int>>
            {
                {
                    "evens", [
                        2,
                        4,
                        6
                    ]
                },
                {
                    "odds", [
                        1,
                        3,
                        5
                    ]
                }
            },
            NestedDict = new Dictionary<string, Dictionary<string, int>>
            {
                { "group1", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
                { "group2", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } }
            }
        };

        // Act
        var result = source.AdaptViaReflection<DictionaryCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.StringIntDict.Should().NotBeNull();
        result.StringIntDict.Count.Should().Be(3);
        result.StringIntDict["one"].Should().Be(1);
        result.IntStringDict.Should().NotBeNull();
        result.IntStringDict[1].Should().Be("one");
        result.StringListDict.Should().NotBeNull();
        result.StringListDict["evens"].Count.Should().Be(3);
        result.NestedDict.Should().NotBeNull();
        result.NestedDict.Count.Should().Be(2);
    }

    [Fact]
    public void AdaptViaReflection_EmptyDictionaries_ShouldMapCorrectly()
    {
        // Arrange
        var source = new DictionaryCollectionsSource
        {
            StringIntDict = new Dictionary<string, int>(),
            IntStringDict = new Dictionary<int, string>()
        };

        // Act
        var result = source.AdaptViaReflection<DictionaryCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.StringIntDict.Should().BeEmpty();
        result.IntStringDict.Should().BeEmpty();
    }

    [Fact]
    public void AdaptViaReflection_SetCollections_ShouldMapCorrectly()
    {
        // Arrange
        var source = new SetCollectionsSource
        {
            IntHashSet =
            [
                1,
                2,
                3,
                4,
                5
            ],
            StringHashSet =
            [
                "apple",
                "banana",
                "cherry"
            ],
            IntSortedSet =
            [
                5,
                3,
                1,
                4,
                2
            ]
        };

        // Act
        var result = source.AdaptViaReflection<SetCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntHashSet.Should().NotBeNull();
        result.IntHashSet.Count.Should().Be(5);
        result.IntHashSet.Should().Contain(3);
        result.StringHashSet.Should().Contain("banana");
        result.IntSortedSet.Should().NotBeNull();
        result.IntSortedSet.Count.Should().Be(5);
    }

    [Fact]
    public void AdaptViaReflection_IEnumerableCollections_ShouldMapCorrectly()
    {
        // Arrange
        var source = new IEnumerableCollectionsSource
        {
            IntEnumerable = new List<int> { 1, 2, 3 },
            StringCollection = new List<string> { "a", "b", "c" },
            DoubleList = new List<double> { 1.1, 2.2, 3.3 },
            ReadOnlyIntList = new List<int> { 10, 20, 30 },
            ReadOnlyStringCollection = new List<string> { "x", "y", "z" }
        };

        // Act
        var result = source.AdaptViaReflection<IEnumerableCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntEnumerable.Should().NotBeNull();
        result.StringCollection.Should().NotBeNull();
        result.DoubleList.Should().NotBeNull();
        result.ReadOnlyIntList.Should().NotBeNull();
        result.ReadOnlyStringCollection.Should().NotBeNull();
    }

    [Fact]
    public void AdaptViaReflection_NullCollections_ShouldMapNulls()
    {
        // Arrange
        var source = new ListCollectionsSource
        {
            IntList = null,
            StringList = null,
            DoubleList = null,
            NestedIntList = null
        };

        // Act
        var result = source.AdaptViaReflection<ListCollectionsDest>();

        // Assert
        result.Should().NotBeNull();
        result.IntList.Should().BeNull();
        result.StringList.Should().BeNull();
        result.DoubleList.Should().BeNull();
        result.NestedIntList.Should().BeNull();
    }

    [Fact]
    public void AdaptViaReflection_ListWithDuplicates_ShouldMapAll()
    {
        // Arrange
        var source = new ListCollectionsSource
        {
            IntList =
            [
                1,
                1,
                2,
                2,
                3,
                3
            ],
            StringList =
            [
                "a",
                "a",
                "b",
                "b"
            ]
        };

        // Act
        var result = source.AdaptViaReflection<ListCollectionsDest>();

        // Assert
        result.IntList.Count.Should().Be(6);
        result.StringList.Count.Should().Be(4);
    }
}