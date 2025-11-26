using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class SimpleMappingBenchmark
{
    private BasicSource1 _basicSource;
    private IMapper _autoMapper;
    private Mapster.TypeAdapterConfig _mapsterConfig;
    private Mappers.TestMapper _mapperly;

    [GlobalSetup]
    public void Setup()
    {
        _basicSource = new BasicSource1
        {
            Id = "test-id-123",
            Name = "Test Name",
            Count = 42
        };

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BasicSource1, BasicDest>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new Mapster.TypeAdapterConfig();
        _mapsterConfig.NewConfig<BasicSource1, BasicDest>();

        // Setup Mapperly
        _mapperly = new Mappers.TestMapper();
    }

    [Benchmark(Baseline = true)]
    public BasicDest GenAdapt()
    {
        return _basicSource.Adapt<BasicDest>();
    }

    [Benchmark]
    public BasicDest AutoMapper()
    {
        return _autoMapper.Map<BasicDest>(_basicSource);
    }

    [Benchmark]
    public BasicDest MapsterBenchmark()
    {
        return Mapster.TypeAdapter.Adapt<BasicDest>(_basicSource, _mapsterConfig);
    }

    [Benchmark]
    public BasicDest Mapperly()
    {
        return _mapperly.MapToBasicDest(_basicSource);
    }

    [Benchmark]
    public BasicFacetDest Facet()
    {
        return _basicSource.ToFacet<BasicSource1, BasicFacetDest>();
    }
}
