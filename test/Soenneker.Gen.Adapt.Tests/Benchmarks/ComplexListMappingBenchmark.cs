using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Gen.Adapt.Tests.Dtos;
using Facet.Extensions;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class ComplexListMappingBenchmark
{
    private ComplexListSource _complexListSource;
    private IMapper _autoMapper;
    private Mapster.TypeAdapterConfig _mapsterConfig;
    private ComplexListTestMapper _mapperly;

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

        _complexListSource = new ComplexListSource
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
            cfg.CreateMap<BasicSource, BasicDest>();
            cfg.CreateMap<NestedSource, NestedDest>();
            cfg.CreateMap<ComplexListSource, ComplexListDest>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new Mapster.TypeAdapterConfig();
        _mapsterConfig.NewConfig<BasicSource, BasicDest>();
        _mapsterConfig.NewConfig<NestedSource, NestedDest>();
        _mapsterConfig.NewConfig<ComplexListSource, ComplexListDest>();

        // Setup Mapperly
        _mapperly = new ComplexListTestMapper();
    }

    [Benchmark(Baseline = true)]
    public ComplexListDest GenAdapt()
    {
        return _complexListSource.Adapt<ComplexListDest>();
    }

    [Benchmark]
    public ComplexListDest AutoMapper()
    {
        return _autoMapper.Map<ComplexListDest>(_complexListSource);
    }

    [Benchmark]
    public ComplexListDest MapsterBenchmark()
    {
        return Mapster.TypeAdapter.Adapt<ComplexListDest>(_complexListSource, _mapsterConfig);
    }

    [Benchmark]
    public ComplexListDest Mapperly()
    {
        return _mapperly.MapToComplexListDest(_complexListSource);
    }

    [Benchmark]
    public ComplexListFacetDest Facet()
    {
        return _complexListSource.ToFacet<ComplexListFacetDest>();
    }
}

// Mapperly mapper class
[Riok.Mapperly.Abstractions.Mapper]
public partial class ComplexListTestMapper
{
    public partial BasicDest MapToBasicDest(BasicSource source);
    public partial NestedDest MapToNestedDest(NestedSource source);
    public partial ComplexListDest MapToComplexListDest(ComplexListSource source);
}