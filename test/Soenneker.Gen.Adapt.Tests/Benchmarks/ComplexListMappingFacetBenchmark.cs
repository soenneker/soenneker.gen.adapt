using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Facet.Extensions;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class ComplexListMappingFacetBenchmark
{
    private ComplexListSource2 _complexListSource;
    private IMapper _autoMapper;
    private Mapster.TypeAdapterConfig _mapsterConfig;
    private Mappers.ComplexListTestMapper _mapperly;

    [GlobalSetup]
    public void Setup()
    {
        var basicSource = new BasicSource
        {
            Id = "test-id-123",
            Name = "Test Name",
            Count = 42
        };

        var nestedSource = new NestedSource
        {
            Name = "Nested Test",
            Child = basicSource
        };

        _complexListSource = new ComplexListSource2
        {
            NestedItems =
            [
                nestedSource,

                new NestedSource { Name = "Second Item", Child = basicSource },

                new NestedSource { Name = "Third Item", Child = basicSource }
            ]
        };

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ComplexListSource2, ComplexListFacetDestComparison>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new Mapster.TypeAdapterConfig();
        _mapsterConfig.NewConfig<ComplexListSource2, ComplexListFacetDestComparison>();

        // Setup Mapperly
        _mapperly = new Mappers.ComplexListTestMapper();
    }

    [Benchmark(Baseline = true)]
    public ComplexListFacetDestComparison GenAdapt()
    {
        return _complexListSource.Adapt<ComplexListFacetDestComparison>();
    }

    [Benchmark]
    public ComplexListFacetDestComparison AutoMapper()
    {
        return _autoMapper.Map<ComplexListFacetDestComparison>(_complexListSource);
    }

    [Benchmark]
    public ComplexListFacetDestComparison MapsterBenchmark()
    {
        return Mapster.TypeAdapter.Adapt<ComplexListFacetDestComparison>(_complexListSource, _mapsterConfig);
    }

    [Benchmark]
    public ComplexListFacetDestComparison Mapperly()
    {
        return _mapperly.MapToComplexListFacetDestComparison(_complexListSource);
    }

    [Benchmark]
    public ComplexListFacetDest Facet()
    {
        return _complexListSource.ToFacet<ComplexListSource2, ComplexListFacetDest>();
    }
}