using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet.Extensions;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
public class SimpleMappingBenchmark
{
    private BasicSource _basicSource;
    private IMapper _autoMapper;
    private TypeAdapterConfig _mapsterConfig;
    private TestMapper _mapperly;

    [GlobalSetup]
    public void Setup()
    {
        _basicSource = new BasicSource
        {
            Id = "test-id-123",
            Name = "Test Name",
            Count = 42
        };

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
        _mapperly = new TestMapper();
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
    public BasicDest Mapster()
    {
        return _basicSource.Adapt<BasicDest>(_mapsterConfig);
    }

    [Benchmark]
    public BasicDest Mapperly()
    {
        return _mapperly.MapToBasicDest(_basicSource);
    }

    [Benchmark]
    public BasicFacetDest Facet()
    {
        return _basicSource.ToFacet<BasicFacetDest>();
    }
}

// Mapperly mapper class
[Riok.Mapperly.Abstractions.Mapper]
public partial class TestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
}
