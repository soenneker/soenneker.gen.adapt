using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class RuntimePerformanceTests
{
    private static List<EnumerableMapDestination>? _allocationSink;

    [Test]
    public void Generated_struct_collection_mapping_allocates_only_list_and_backing_storage()
    {
        IEnumerable<EnumerableMapSource> source = new List<EnumerableMapSource> { new() { Id = 1 }, new() { Id = 2 }, new() { Id = 3 } };
        for (int i = 0; i < 1000; i++) _allocationSink = source.Adapt<List<EnumerableMapDestination>>();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) _allocationSink = new List<EnumerableMapDestination>(3);
        long baseline = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) _allocationSink = source.Adapt<List<EnumerableMapDestination>>();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        allocated.Should().Be(baseline);
        _allocationSink!.Select(x => x.Id).Should().Equal(1, 2, 3);
    }

    [Test]
    public void Collection_properties_are_read_once_and_copies_are_independent()
    {
        var source = new PerformanceSource();
        var result = source.Adapt<PerformanceDestination>();
        source.Reads.Should().Be(3);
        result.Values.Should().Equal(1, 2, 3);
        result.Items.Select(x => x.Id).Should().Equal(7, 9);
        result.Items.Capacity.Should().Be(2);
        result.Lookup.Should().ContainKey("one").WhoseValue.Should().Be(1);
        result.Values[0] = 99;
        source.Values[0].Should().Be(1);
    }

    [Test]
    public void Empty_array_properties_reuse_empty_storage()
    {
        var source = new EmptyArraySource { Values = [], Items = [] };
        var result = source.Adapt<EmptyArrayDestination>();
        result.Values.Should().BeSameAs(Array.Empty<int>());
        result.Items.Should().BeSameAs(Array.Empty<int>());
    }

    [Test]
    public void Enum_formatting_handles_names_aliases_flags_and_unknown_values()
    {
        foreach (var value in new[] { PerformanceFlags.None, PerformanceFlags.Read, PerformanceFlags.All,
                     PerformanceFlags.Read | PerformanceFlags.Execute, (PerformanceFlags)123, (PerformanceFlags)(-2) })
        {
            new FlagSource { Value = value }.Adapt<FlagDestination>().Value.Should().Be(value.ToString());
        }
        new AliasSource { Value = AliasedState.Ready }.Adapt<FlagDestination>().Value.Should().Be("Ready");
        new AliasSource { Value = (AliasedState)99 }.Adapt<FlagDestination>().Value.Should().Be("99");
        new UnsignedFlagSource { Value = WideFlags.High | WideFlags.Low }.Adapt<FlagDestination>().Value.Should().Be("Low, High");
        new SignedFlagSource { Value = SignedFlags.High | SignedFlags.Low }.Adapt<FlagDestination>().Value.Should().Be("Low, High");
        new EnumArraySource { Values = [PerformanceFlags.Read, PerformanceFlags.Execute] }.Adapt<EnumArrayDestination>()
            .Values.Should().Equal("Read", "Execute");
    }

    [Test]
    public void Struct_mapping_with_named_enum_allocates_nothing_after_warmup()
    {
        var source = new EnumStructSource { Value = PerformanceFlags.Read };
        for (int i = 0; i < 1000; i++) source.Adapt<EnumStructDestination>();
        long before = GC.GetAllocatedBytesForCurrentThread();
        EnumStructDestination result = default;
        for (int i = 0; i < 1000; i++) result = source.Adapt<EnumStructDestination>();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        allocated.Should().Be(0);
        result.Value.Should().Be("Read");
    }

    [Test]
    public void Cross_shape_runtime_element_conversion_requires_explicit_reflection_and_enumerates_once()
    {
        int enumerations = 0;
        IEnumerable<int> Values() { enumerations++; yield return 1; }
        Action generated = () => Values().Adapt<List<string>, int>();
        generated.Should().Throw<NotSupportedException>();
        enumerations.Should().Be(0);
        Values().AdaptViaReflection<List<string>>().Should().Equal("1");
        enumerations.Should().Be(1);
    }

    [Test]
    public void Runtime_fallback_maps_unknown_structs_nested_arrays_and_dictionaries()
    {
        var input = new RuntimeOnlySource { Id = 7 };
        MapRuntime<RuntimeOnlySource, RuntimeOnlyDestination>(input).Id.Should().Be(7);
        object array = new[] { input, new RuntimeOnlySource { Id = 9 } };
        array.Adapt<RuntimeOnlyDestination[]>().Select(x => x.Id).Should().Equal(7, 9);
        object dictionary = new Dictionary<string, RuntimeOnlySource> { ["first"] = input };
        dictionary.Adapt<Dictionary<string, RuntimeOnlyDestination>>()["first"].Id.Should().Be(7);
        var nested = new RuntimeArraySource { Values = [input] };
        MapRuntime<RuntimeArraySource, RuntimeArrayDestination>(nested).Values[0].Id.Should().Be(7);
    }

    [Test]
    public void Generic_wrapper_with_an_unknown_destination_requires_explicit_reflection()
    {
        var source = new EnumerableMapSource { Id = 12 };
        Action generated = () => MapKnownSource<RuntimeOnlyDestination>(source);
        generated.Should().Throw<NotSupportedException>();
        source.AdaptViaReflection<RuntimeOnlyDestination>().Id.Should().Be(12);
    }

    private static TDestination MapRuntime<TSource, TDestination>(TSource source) => source.Adapt<TDestination>();
    private static TDestination MapKnownSource<TDestination>(EnumerableMapSource source) => source.Adapt<TDestination>();
}

public sealed class PerformanceSource
{
    private readonly int[] _values = [1, 2, 3];
    private readonly EnumerableMapSource[] _items = [new() { Id = 7 }, new() { Id = 9 }];
    private readonly Dictionary<string, int> _lookup = new() { ["one"] = 1 };
    public int Reads;
    public int[] Values { get { Reads++; return _values; } }
    public EnumerableMapSource[] Items { get { Reads++; return _items; } }
    public Dictionary<string, int> Lookup { get { Reads++; return _lookup; } }
}
public sealed class PerformanceDestination
{
    public int[] Values { get; set; } = [];
    public List<EnumerableMapDestination> Items { get; set; } = null!;
    public Dictionary<string, int> Lookup { get; set; } = null!;
}
public sealed class EmptyArraySource { public int[] Values { get; set; } = []; public List<int> Items { get; set; } = []; }
public sealed class EmptyArrayDestination { public int[] Values { get; set; } = []; public int[] Items { get; set; } = []; }
[Flags] public enum PerformanceFlags { None = 0, Read = 1, Write = 2, Execute = 4, All = -1 }
public enum AliasedState { Ready = 1, Alias = 1 }
[Flags] public enum WideFlags : ulong { Low = 1, High = 1UL << 63 }
[Flags] public enum SignedFlags : int { Low = 1, High = int.MinValue }
public sealed class FlagSource { public PerformanceFlags Value { get; set; } }
public sealed class AliasSource { public AliasedState Value { get; set; } }
public sealed class UnsignedFlagSource { public WideFlags Value { get; set; } }
public sealed class SignedFlagSource { public SignedFlags Value { get; set; } }
public sealed class FlagDestination { public string Value { get; set; } = ""; }
public struct EnumStructSource { public PerformanceFlags Value { get; set; } }
public struct EnumStructDestination { public string Value { get; set; } }
public sealed class EnumArraySource { public PerformanceFlags[] Values { get; set; } = []; }
public sealed class EnumArrayDestination { public string[] Values { get; set; } = []; }
public struct RuntimeOnlySource { public int Id { get; set; } }
public struct RuntimeOnlyDestination { public int Id { get; set; } }
public sealed class RuntimeArraySource { public RuntimeOnlySource[] Values { get; set; } = []; }
public sealed class RuntimeArrayDestination { public RuntimeOnlyDestination[] Values { get; set; } = []; }
