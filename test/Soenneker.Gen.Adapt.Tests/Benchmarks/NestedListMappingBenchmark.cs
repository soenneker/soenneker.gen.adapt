using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet.Extensions;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Riok.Mapperly.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class NestedListMappingBenchmark
{
    private List<NestedSource> _nestedList;
    private IMapper _autoMapper;
    private TypeAdapterConfig _mapsterConfig;
    private NestedListTestMapper _mapperly;

    [GlobalSetup]
    public void Setup()
    {
        _nestedList = Enumerable.Range(1, 100)
            .Select(i => new NestedSource
            {
                Name = $"Nested {i}",
                Child = new BasicSource
                {
                    Id = $"child-id-{i}",
                    Name = $"Child Name {i}",
                    Count = i
                }
            })
            .ToList();

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BasicSource, BasicDest>();
            cfg.CreateMap<NestedSource, NestedDest>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new TypeAdapterConfig();
        _mapsterConfig.NewConfig<BasicSource, BasicDest>();
        _mapsterConfig.NewConfig<NestedSource, NestedDest>();

        // Setup Mapperly
        _mapperly = new NestedListTestMapper();
    }

    [Benchmark(Baseline = true)]
    public List<NestedDest> GenAdapt()
    {
        return _nestedList.Adapt<List<NestedDest>>();
    }

    [Benchmark]
    public List<NestedDest> AutoMapper()
    {
        return _autoMapper.Map<List<NestedDest>>(_nestedList);
    }

    [Benchmark]
    public List<NestedDest> MapsterBenchmark()
    {
        return TypeAdapter.Adapt<List<NestedDest>>(_nestedList, _mapsterConfig);
    }

    [Benchmark]
    public List<NestedDest> Mapperly()
    {
        return _mapperly.MapToNestedDestList(_nestedList);
    }

    [Benchmark]
    public List<NestedFacetDest> Facet()
    {
        return _nestedList.Select(x => x.ToFacet<NestedFacetDest>()).ToList();
    }

}

// Mapperly mapper class
[Riok.Mapperly.Abstractions.Mapper]
public partial class NestedListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial NestedDest MapToNestedDest(NestedSource source);
    public partial List<NestedDest> MapToNestedDestList(List<NestedSource> source);
}
