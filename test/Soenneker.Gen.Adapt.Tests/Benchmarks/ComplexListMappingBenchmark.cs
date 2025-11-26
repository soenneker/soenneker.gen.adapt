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
    private ComplexListSource1 _complexListSource;
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

        _complexListSource = new ComplexListSource1
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
            cfg.CreateMap<ComplexListSource1, ComplexListDest>();
        }, new NullLoggerFactory());
        _autoMapper = config.CreateMapper();

        // Setup Mapster
        _mapsterConfig = new Mapster.TypeAdapterConfig();
        _mapsterConfig.NewConfig<BasicSource, BasicDest>();
        _mapsterConfig.NewConfig<NestedSource, NestedDest>();
        _mapsterConfig.NewConfig<ComplexListSource1, ComplexListDest>();

        // Setup Mapperly
        _mapperly = new Mappers.ComplexListTestMapper();
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
}