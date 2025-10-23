using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet.Extensions;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class NestedMappingBenchmark
{
    private NestedSource _nestedSource;
    private IMapper _autoMapper;
    private TypeAdapterConfig _mapsterConfig;
    private NestedTestMapper _mapperly;

    [GlobalSetup]
    public void Setup()
    {
        var basicSource = new BasicSource
        {
            Id = "test-id-123",
            Name = "Test Name",
            Count = 42
        };

        _nestedSource = new NestedSource
        {
            Name = "Nested Test",
            Child = basicSource
        };

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
        _mapperly = new NestedTestMapper();
    }

    [Benchmark(Baseline = true)]
    public NestedDest GenAdapt()
    {
        return _nestedSource.Adapt<NestedDest>();
    }

    [Benchmark]
    public NestedDest AutoMapper()
    {
        return _autoMapper.Map<NestedDest>(_nestedSource);
    }

    [Benchmark]
    public NestedDest Mapster()
    {
        return _nestedSource.Adapt<NestedDest>(_mapsterConfig);
    }

    [Benchmark]
    public NestedDest Mapperly()
    {
        return _mapperly.MapToNestedDest(_nestedSource);
    }

    [Benchmark]
    public NestedFacetDest Facet()
    {
        return _nestedSource.ToFacet<NestedFacetDest>();
    }

}

// Mapperly mapper class
[Riok.Mapperly.Abstractions.Mapper]
public partial class NestedTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial NestedDest MapToNestedDest(NestedSource source);
}
