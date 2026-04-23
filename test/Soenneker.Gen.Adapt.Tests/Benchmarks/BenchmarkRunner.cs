namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

public class BenchmarkRunner : BenchmarkTest
{
    public BenchmarkRunner() : base()
    {
    }

   // [LocalOnly]
    public async ValueTask ComplexListMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ComplexListMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [LocalOnly]
    public async ValueTask ComplexListMappingFacetBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ComplexListMappingFacetBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

  //  [LocalOnly]
    public async ValueTask LargeListMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<LargeListMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

  //  [LocalOnly]
    public async ValueTask SimpleMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<SimpleMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [LocalOnly]
    public async ValueTask NestedMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<NestedMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

    //[LocalOnly]
    public async ValueTask NestedFacetMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<NestedFacetMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

    //[LocalOnly]
    public async ValueTask NestedListMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<NestedListMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }
}



