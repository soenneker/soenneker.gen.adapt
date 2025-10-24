using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class LargeListMappingBenchmark
{
    private List<BasicSource> _basicList;
    private IMapper _autoMapper;
    private Mapster.TypeAdapterConfig _mapsterConfig;
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
        _mapsterConfig = new Mapster.TypeAdapterConfig();
        _mapsterConfig.NewConfig<BasicSource, BasicDest>();

        // Setup Mapperly
        _mapperly = new LargeListTestMapper();
    }

    [Benchmark(Baseline = true)]
    public List<BasicDest> GenAdapt()
    {
        return _basicList.Adapt<List<BasicDest>>();
    }

    //[Benchmark]
    //public List<BasicDest> AutoMapper()
    //{
    //    return _autoMapper.Map<List<BasicDest>>(_basicList);
    //}

    //[Benchmark]
    //public List<BasicDest> MapsterBenchmark()
    //{
    //    return Mapster.TypeAdapter.Adapt<List<BasicDest>>(_basicList, _mapsterConfig);
    //}

    [Benchmark]
    public List<BasicDest> Mapperly()
    {
        return _mapperly.MapToBasicDestList(_basicList);
    }
    
}

// Mapperly mapper class
[Riok.Mapperly.Abstractions.Mapper]
public partial class LargeListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial List<BasicDest> MapToBasicDestList(List<BasicSource> source);
}

