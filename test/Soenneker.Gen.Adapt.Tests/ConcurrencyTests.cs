using Soenneker.Tests.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class ConcurrencyTests : UnitTest
{
    public ConcurrencyTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_ParallelAdaptations_ShouldAllSucceed()
    {
        // Arrange
        var sources = Enumerable.Range(1, 1000)
            .Select(i => new BasicSource { Id = $"id_{i}", Name = $"name_{i}", Count = i })
            .ToList();

        // Act
        var results = new BasicDest[1000];
        Parallel.For(0, 1000, i =>
        {
            results[i] = sources[i].Adapt<BasicDest>();
        });

        // Assert
        for (int i = 0; i < 1000; i++)
        {
            results[i].Id.Should().Be($"id_{i + 1}");
            results[i].Name.Should().Be($"name_{i + 1}");
            results[i].Count.Should().Be(i + 1);
        }
    }

    [Fact]
    public void Adapt_ConcurrentListAdaptations_ShouldAllSucceed()
    {
        // Arrange
        var source = Enumerable.Range(1, 100).ToList();

        // Act
        var results = new List<int>[100];
        Parallel.For(0, 100, i =>
        {
            results[i] = source.Adapt();
        });

        // Assert
        results.Should().AllSatisfy(result =>
        {
            result.Count.Should().Be(100);
            result.Sum().Should().Be(5050);
        });
    }

    [Fact]
    public void Adapt_ConcurrentDictionaryAdaptations_ShouldAllSucceed()
    {
        // Arrange
        var source = new Dictionary<string, int>
        {
            { "a", 1 }, { "b", 2 }, { "c", 3 }, { "d", 4 }, { "e", 5 }
        };

        // Act
        var results = new Dictionary<string, int>[100];
        Parallel.For(0, 100, i =>
        {
            results[i] = source.Adapt();
        });

        // Assert
        results.Should().AllSatisfy(result =>
        {
            result.Count.Should().Be(5);
            result["a"].Should().Be(1);
            result["e"].Should().Be(5);
        });
    }

    [Fact]
    public void Adapt_SequentialAdaptations_10000Times_ShouldNotDegradePerformance()
    {
        // Arrange
        var source = new BasicSource { Id = "test", Name = "performance", Count = 42 };

        // Act
        var results = new BasicDest[10000];
        for (int i = 0; i < 10000; i++)
        {
            results[i] = source.Adapt<BasicDest>();
        }

        // Assert
        results.Should().AllSatisfy(r =>
        {
            r.Id.Should().Be("test");
            r.Name.Should().Be("performance");
            r.Count.Should().Be(42);
        });
        
        // Verify they're all different instances
        for (int i = 0; i < 100; i++)
        {
            for (int j = i + 1; j < 100; j++)
            {
                results[i].Should().NotBeSameAs(results[j]);
            }
        }
    }

    [Fact]
    public void Adapt_HugeNestedListStructure_ShouldMapAll()
    {
        // Arrange
        var sources = new List<MultiListSource>();
        for (int i = 0; i < 100; i++)
        {
            sources.Add(new MultiListSource
            {
                Numbers = Enumerable.Range(i * 100, 100).ToList(),
                Tags = Enumerable.Range(i * 100, 100).Select(n => $"tag_{n}").ToList(),
                Items = new List<BasicSource>()
            });
        }

        // Act
        var results = new List<MultiListDest>();
        foreach (var source in sources)
        {
            results.Add(source.Adapt<MultiListDest>());
        }

        // Assert
        results.Count.Should().Be(100);
        results[0].Numbers[0].Should().Be(0);
        results[99].Numbers[99].Should().Be(9999);
        results[50].Tags[50].Should().Be("tag_5050");
    }

    [Fact]
    public void Adapt_ListOfComplexObjects_1000Items_ShouldMapAll()
    {
        // Arrange
        var sources = Enumerable.Range(1, 1000)
            .Select(i => new NestedSource
            {
                Name = $"Parent_{i}",
                Child = new BasicSource
                {
                    Id = $"child_id_{i}",
                    Name = $"Child_{i}",
                    Count = i
                }
            })
            .ToList();

        // Act
        var results = new List<NestedDest>();
        foreach (var source in sources)
        {
            results.Add(source.Adapt<NestedDest>());
        }

        // Assert
        results.Count.Should().Be(1000);
        results[0].Name.Should().Be("Parent_1");
        results[0].Child.Count.Should().Be(1);
        results[999].Name.Should().Be("Parent_1000");
        results[999].Child.Count.Should().Be(1000);
    }

    [Fact]
    public void Adapt_MixedCollectionTypes_Simultaneously_ShouldAllWork()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        var dict = new Dictionary<string, int> { { "a", 1 } };

        // Act
        List<int> listResult = null;
        Dictionary<string, int> dictResult = null;
        
        Parallel.Invoke(
            () => listResult = list.Adapt(),
            () => dictResult = dict.Adapt()
        );

        // Assert
        listResult.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        dictResult["a"].Should().Be(1);
    }

    [Fact]
    public void Adapt_RecursiveBackAndForth_100Times_ShouldPreserveData()
    {
        // Arrange
        var original = new BasicSource { Id = "test", Name = "recursive", Count = 999 };

        // Act - convert back and forth 100 times (starting with Source, ending with Source after 100 conversions)
        object current = original;
        for (int i = 0; i < 100; i++)
        {
            if (i % 2 == 0)
                current = ((BasicSource)current).Adapt<BasicDest>();
            else
                current = ((BasicDest)current).Adapt<BasicSource>();
        }

        // Assert - after 100 iterations (0-99), last iteration is odd, so ends as BasicSource
        var final = (BasicSource)current;
        final.Id.Should().Be("test");
        final.Name.Should().Be("recursive");
        final.Count.Should().Be(999);
    }

    [Fact]
    public void Adapt_MultipleEnumConversions_InParallel_ShouldAllSucceed()
    {
        // Arrange
        var sources = Enumerable.Range(0, 300)
            .Select(i => new EnumToIntSource { Status = (TestStatus)(i % 3) })
            .ToList();

        // Act
        var results = new EnumToIntDest[300];
        Parallel.For(0, 300, i =>
        {
            results[i] = sources[i].Adapt<EnumToIntDest>();
        });

        // Assert
        for (int i = 0; i < 300; i++)
        {
            results[i].Status.Should().Be(i % 3);
        }
    }

    [Fact]
    public void Adapt_StringToEnum_MultipleThreads_ShouldAllParse()
    {
        // Arrange
        var statusStrings = new[] { "Active", "Pending", "Completed" };
        var sources = Enumerable.Range(0, 300)
            .Select(i => new StringToEnumSource { StatusString = statusStrings[i % 3] })
            .ToList();

        // Act
        var results = new StringToEnumDest[300];
        Parallel.For(0, 300, i =>
        {
            results[i] = sources[i].Adapt<StringToEnumDest>();
        });

        // Assert
        results[0].StatusString.Should().Be(TestStatus.Active);
        results[1].StatusString.Should().Be(TestStatus.Pending);
        results[2].StatusString.Should().Be(TestStatus.Completed);
        results[299].StatusString.Should().Be(TestStatus.Completed);
    }

    [Fact]
    public void Adapt_MixedStructAndClass_InParallel_ShouldWork()
    {
        // Arrange
        var sources = Enumerable.Range(0, 100)
            .Select(i => new MixedStructSource
            {
                Location = new PointStructSource { X = i, Y = i * 2 },
                Label = $"point_{i}"
            })
            .ToList();

        // Act
        var results = new MixedStructDest[100];
        Parallel.For(0, 100, i =>
        {
            results[i] = sources[i].Adapt<MixedStructDest>();
        });

        // Assert
        results[0].Location.X.Should().Be(0);
        results[50].Location.X.Should().Be(50);
        results[50].Location.Y.Should().Be(100);
        results[99].Label.Should().Be("point_99");
    }

    [Fact]
    public void Adapt_Records_InParallel_ShouldWork()
    {
        // Arrange
        var sources = Enumerable.Range(0, 100)
            .Select(i => new PersonRecordSource
            {
                FirstName = $"Person_{i}",
                LastName = $"Last_{i}",
                Age = i
            })
            .ToList();

        // Act
        var results = new PersonRecordDest[100];
        Parallel.For(0, 100, i =>
        {
            results[i] = sources[i].Adapt<PersonRecordDest>();
        });

        // Assert
        results[0].FirstName.Should().Be("Person_0");
        results[50].Age.Should().Be(50);
        results[99].FirstName.Should().Be("Person_99");
    }

    [Fact]
    public void Adapt_DictionaryWithComplexValues_100Items_ShouldMapAll()
    {
        // Arrange
        var source = new Dictionary<string, BasicSource>();
        for (int i = 0; i < 100; i++)
        {
            source[$"key_{i}"] = new BasicSource
            {
                Id = $"id_{i}",
                Name = $"name_{i}",
                Count = i
            };
        }

        // Act
        var result = source.Adapt();

        // Assert
        result.Count.Should().Be(100);
        result["key_0"].Id.Should().Be("id_0");
        result["key_99"].Count.Should().Be(99);
    }
}

