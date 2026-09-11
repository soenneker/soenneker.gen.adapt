using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Soenneker.Gen.Adapt.Generator.Tests;

public sealed class IncrementalTests
{
    [Test]
    public void Fixed_helpers_are_cached_but_namespace_changes_invalidate_them()
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("Probe", [CSharpSyntaxTree.ParseText("public class Source {}")], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new AdaptGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        driver = driver.RunGenerators(compilation);
        var edited = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class Unrelated {}"));
        driver = driver.RunGenerators(edited);
        var run = driver.GetRunResult().Results.Single();
        if (!run.TrackedSteps["HelperNamespace"].SelectMany(s => s.Outputs).All(o => o.Reason == IncrementalStepRunReason.Unchanged) ||
            !run.TrackedOutputSteps["SourceOutput"].SelectMany(s => s.Outputs).Any(o => o.Reason == IncrementalStepRunReason.Cached))
            throw new InvalidOperationException("Unrelated edit rebuilt fixed helpers.");
        driver = driver.RunGeneratorsAndUpdateCompilation(edited.WithAssemblyName("Renamed"), out var output, out var diagnostics);
        if (diagnostics.Concat(output.GetDiagnostics()).Any(d => d.Severity == DiagnosticSeverity.Error))
            throw new InvalidOperationException("Renamed output did not compile.");
        foreach (var result in driver.GetRunResult().Results.Single().GeneratedSources)
            if (!result.SourceText.ToString().Contains("namespace Renamed"))
                throw new InvalidOperationException("Namespace was not updated.");
    }
}
