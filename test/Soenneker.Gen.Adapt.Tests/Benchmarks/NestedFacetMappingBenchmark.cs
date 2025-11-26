using AutoMapper;
using BenchmarkDotNet.Attributes;
using Facet.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class NestedFacetMappingBenchmark
{
    private NestedSource _nestedSource;
    private IMapper _autoMapper;
    private Mapster.TypeAdapterConfig _mapsterConfig;
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
            cfg.CreateMap<NestedSource, NestedFacetDestComparison>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new Mapster.TypeAdapterConfig();
        _mapsterConfig.NewConfig<NestedSource, NestedFacetDestComparison>();

        // Setup Mapperly
        _mapperly = new NestedTestMapper();
    }

    [Benchmark(Baseline = true)]
    public NestedFacetDestComparison GenAdapt()
    {
        return _nestedSource.Adapt<NestedFacetDestComparison>();
    }

    [Benchmark]
    public NestedFacetDestComparison AutoMapper()
    {
        return _autoMapper.Map<NestedFacetDestComparison>(_nestedSource);
    }

    [Benchmark]
    public NestedFacetDestComparison MapsterBenchmark()
    {
        return Mapster.TypeAdapter.Adapt<NestedFacetDestComparison>(_nestedSource, _mapsterConfig);
    }

    [Benchmark]
    public NestedFacetDestComparison Mapperly()
    {
        return _mapperly.MapToNestedFacetDest(_nestedSource);
    }

    [Benchmark]
    public NestedFacetDest Facet()
    {
        return _nestedSource.ToFacet<NestedSource, NestedFacetDest>();
    }
}
