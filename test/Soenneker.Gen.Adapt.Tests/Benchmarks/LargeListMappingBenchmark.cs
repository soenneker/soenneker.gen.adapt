using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet.Extensions;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
public class LargeListMappingBenchmark
{
    private List<BasicSource> _basicList;
    private IMapper _autoMapper;
    private TypeAdapterConfig _mapsterConfig;
    private LargeListTestMapper _mapperly;

    [GlobalSetup]
    public void Setup()
    {
        _basicList = Enumerable.Range(1, 1000)
            .Select(i => new BasicSource
            {
                Id = $"id-{i}",
                Name = $"Name {i}",
                Count = i
            })
            .ToList();

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BasicSource, BasicDest>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new TypeAdapterConfig();
        _mapsterConfig.NewConfig<BasicSource, BasicDest>();

        // Setup Mapperly
        _mapperly = new LargeListTestMapper();
    }

    [Benchmark(Baseline = true)]
    public List<BasicDest> GenAdapt()
    {
        return _basicList.Adapt<List<BasicDest>>();
    }

    [Benchmark]
    public List<BasicDest> AutoMapper()
    {
        return _autoMapper.Map<List<BasicDest>>(_basicList);
    }

    [Benchmark]
    public List<BasicDest> Mapster()
    {
        return _basicList.Adapt<List<BasicDest>>(_mapsterConfig);
    }

    [Benchmark]
    public List<BasicDest> Mapperly()
    {
        return _mapperly.MapToBasicDestList(_basicList);
    }

    [Benchmark]
    public List<BasicFacetDest> Facet()
    {
        return _basicList.Select(x => x.ToFacet<BasicFacetDest>()).ToList();
    }

}

// Mapperly mapper class
[Riok.Mapperly.Abstractions.Mapper]
public partial class LargeListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial List<BasicDest> MapToBasicDestList(List<BasicSource> source);
}
