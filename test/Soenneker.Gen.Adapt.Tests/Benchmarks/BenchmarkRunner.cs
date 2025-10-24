using BenchmarkDotNet.Reports;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Tests.Benchmark;
using System.Threading.Tasks;
using Soenneker.Facts.Local;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Benchmarks;

public class BenchmarkRunner : BenchmarkTest
{
    public BenchmarkRunner(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

//[LocalFact]
    public async ValueTask ComplexListMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ComplexListMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [LocalFact]
    public async ValueTask LargeListMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<LargeListMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [LocalFact]
    public async ValueTask SimpleMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<SimpleMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [LocalFact]
    public async ValueTask NestedMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<NestedMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [LocalFact]
    public async ValueTask NestedListMappingBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<NestedListMappingBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }
}