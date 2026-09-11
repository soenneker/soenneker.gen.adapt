using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class EnumerableMappingRegressionTests
{
    [Test]
    public void Interface_backed_collections_map_in_order_with_exact_capacity()
    {
        var values = new List<EnumerableMapSource> { new() { Id = 3 }, new() { Id = 1 }, new() { Id = 2 } };
        IEnumerable<EnumerableMapSource>[] sources = [values, values.ToArray(), new HashSet<EnumerableMapSource>(values), new ReadOnlyValues(values)];
        foreach (var source in sources)
        {
            var result = source.Adapt<List<EnumerableMapDestination>>();
            result.Select(x => x.Id).Should().Equal(source.Select(x => x.Id));
            result.Capacity.Should().Be(values.Count);
        }

        IReadOnlyList<EnumerableMapSource> readOnly = values;
        readOnly.Adapt<List<EnumerableMapDestination>>().Select(x => x.Id).Should().Equal(3, 1, 2);
        IList<EnumerableMapSource> list = values;
        list.Adapt<List<EnumerableMapDestination>>().Select(x => x.Id).Should().Equal(3, 1, 2);
    }

    [Test]
    public void Unknown_count_is_enumerated_once()
    {
        int enumerations = 0;
        IEnumerable<EnumerableMapSource> Values()
        {
            enumerations++;
            yield return new() { Id = 9 };
            yield return new() { Id = 4 };
        }
        Values().Adapt<List<EnumerableMapDestination>>().Select(x => x.Id).Should().Equal(9, 4);
        enumerations.Should().Be(1);
    }

    [Test]
    public void List_subclass_keeps_its_custom_interface_enumeration()
    {
        IEnumerable<EnumerableMapSource> values = new CustomList { new() { Id = 1 } };
        values.Adapt<List<EnumerableMapDestination>>().Select(x => x.Id).Should().Equal(42, 43);
    }

    [Test]
    public void Covariant_arrays_and_null_elements_keep_mapping_behavior()
    {
        IEnumerable<EnumerableMapBase> values = new EnumerableMapDerived[] { new() { Id = 7 }, null! };
        var result = values.Adapt<List<EnumerableMapReferenceDestination>>();
        result[0].Id.Should().Be(7);
        result[1].Should().BeNull();
    }

    [Test]
    public void Empty_and_null_interface_sources_keep_mapping_behavior()
    {
        IEnumerable<EnumerableMapSource>[] emptySources = [new List<EnumerableMapSource>(), Array.Empty<EnumerableMapSource>(), new ReadOnlyValues([])];
        foreach (var source in emptySources)
        {
            var result = source.Adapt<List<EnumerableMapDestination>>();
            result.Should().BeEmpty();
            result.Capacity.Should().Be(0);
        }
        IEnumerable<EnumerableMapSource> missing = null!;
        // Object/collection mapping overloads preserve null source values.
        missing.Adapt<List<EnumerableMapDestination>>().Should().BeNull();
    }

    private sealed class ReadOnlyValues(List<EnumerableMapSource> values) : IReadOnlyCollection<EnumerableMapSource>
    {
        public int Count => values.Count;
        public IEnumerator<EnumerableMapSource> GetEnumerator() => values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CustomList : List<EnumerableMapSource>, IEnumerable<EnumerableMapSource>
    {
        IEnumerator<EnumerableMapSource> IEnumerable<EnumerableMapSource>.GetEnumerator()
        {
            yield return new() { Id = 42 };
            yield return new() { Id = 43 };
        }
    }
}

public struct EnumerableMapSource { public int Id { get; set; } }
public struct EnumerableMapDestination { public int Id { get; set; } }
public class EnumerableMapBase { public int Id { get; set; } }
public sealed class EnumerableMapDerived : EnumerableMapBase;
public sealed class EnumerableMapReferenceDestination { public int Id { get; set; } }
