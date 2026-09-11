using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class CollectionPerformanceRegressionTests
{
    [Test]
    public void Dictionary_copies_preserve_comparer_removed_entries_and_independence()
    {
        var source = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["first"] = 1, ["removed"] = 2, ["last"] = 3 };
        source.Remove("removed");
        var copies = new[] { source.Adapt(), ((IDictionary<string, int>)source).Adapt(), ((IReadOnlyDictionary<string, int>)source).Adapt() };
        foreach (var copy in copies)
        {
            copy.Comparer.Should().BeSameAs(source.Comparer);
            copy.Keys.Should().Equal(source.Keys);
            copy["FIRST"].Should().Be(1);
            copy["first"] = 7;
            source["first"].Should().Be(1);
        }
        source.Clear();
        source.Adapt().Comparer.Should().BeSameAs(source.Comparer);
    }

    [Test]
    public void Collection_clones_keep_contents_and_do_not_alias_sources()
    {
        var source = new List<int> { 1, 2, 3 };
        ((IList<int>)source).Adapt().Should().Equal(source).And.NotBeSameAs(source);
        ((ICollection<int>)source).Adapt().Should().Equal(source).And.NotBeSameAs(source);
        ((IEnumerable<int>)new HashSet<int>(source)).Adapt().Should().Equal(source);
        var readOnly = new ReadOnlyOnly(source);
        ((IReadOnlyList<int>)readOnly).Adapt().Should().Equal(source);
        ((IReadOnlyCollection<int>)readOnly).Adapt().Should().Equal(source);
    }

    [Test]
    public void Same_element_cross_shape_conversions_preserve_order_and_shape()
    {
        IEnumerable<int> source = YieldValues();
        source.Adapt<int[], int>().Should().Equal(1, 2, 3);
        source.Adapt<List<int>, int>().Should().Equal(1, 2, 3);
        source.Adapt<IReadOnlyList<int>, int>().Should().Equal(1, 2, 3);
        source.Adapt<HashSet<int>, int>().Should().BeEquivalentTo(new[] { 1, 2, 3 });
        source.Adapt<SortedSet<int>, int>().Should().Equal(1, 2, 3);
        source.Adapt<LinkedList<int>, int>().Should().Equal(1, 2, 3);
        source.Adapt<Queue<int>, int>().Should().Equal(1, 2, 3);
        source.Adapt<Stack<int>, int>().Should().Equal(3, 2, 1);
        source.Adapt<Collection<int>, int>().Should().Equal(1, 2, 3);
        source.Adapt<ObservableCollection<int>, int>().Should().Equal(1, 2, 3);
        Array.Empty<int>().Adapt<int[], int>().Should().BeEmpty();
        var item = new object();
        new[] { item }.Adapt<List<object>, object>()[0].Should().BeSameAs(item);
    }

    [Test]
    public void Optimized_collection_paths_reject_null()
    {
        Action list = () => ((IList<int>)null!).Adapt();
        Action dictionary = () => ((Dictionary<string, int>)null!).Adapt();
        Action array = () => ((IEnumerable<int>)null!).Adapt<int[], int>();
        list.Should().Throw<ArgumentNullException>();
        dictionary.Should().Throw<ArgumentNullException>();
        array.Should().Throw<ArgumentNullException>();
    }

    private static IEnumerable<int> YieldValues() { yield return 1; yield return 2; yield return 3; }

    private sealed class ReadOnlyOnly(List<int> values) : IReadOnlyList<int>
    {
        public int Count => values.Count;
        public int this[int index] => values[index];
        public IEnumerator<int> GetEnumerator() => values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
