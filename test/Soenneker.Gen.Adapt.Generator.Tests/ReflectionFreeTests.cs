using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Soenneker.Gen.Adapt.Generator.Tests;

public sealed class ReflectionFreeTests
{
    [Test]
    public void Conditional_access_generates_typed_mappings_and_preserves_null_behavior()
    {
        foreach (string expression in new[] { "source?.Adapt<Destination>()", "holder.Value?.Adapt<Destination>()", "holder?.Value?.Adapt<Destination>()", "source?.Adapt<Source>()?.Adapt<Destination>()", "optional?.Adapt<Destination>()" })
        {
            var (output, diagnostics, _) = Generate("""
                using Probe;
                public class Source { public int Value { get; set; } }
                public struct ValueSource { public int Value { get; set; } }
                public class Destination { public int Value { get; set; } }
                public class Holder { public Source Value { get; set; } }
                public static class Calls
                {
                    public static int Map(bool present)
                    {
                        Source source = present ? new Source { Value = 42 } : null;
                        Holder holder = new Holder { Value = source };
                        ValueSource? optional = present ? new ValueSource { Value = 42 } : null;
                """ + "return (" + expression + " ?? new Destination()).Value; } }");
            AssertNoErrors(output, diagnostics);
            if (diagnostics.Concat(output.GetDiagnostics()).Any(d => d.Id == "SGA005"))
                throw new InvalidOperationException("Conditional mapping unexpectedly warned: " + expression);
            using var stream = new MemoryStream();
            var result = output.Emit(stream);
            if (!result.Success)
                throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
            var assembly = System.Reflection.Assembly.Load(stream.ToArray());
            var map = assembly.GetType("Calls")!.GetMethod("Map")!;
            if (!Equals(map.Invoke(null, [true]), 42) || !Equals(map.Invoke(null, [false]), 0))
                throw new InvalidOperationException("Conditional mapping changed value/null behavior: " + expression);
        }
    }

    [Test]
    public void Generated_mapping_paths_keep_metadata_inspection_in_the_fallback()
    {
        var (output, diagnostics, run) = Generate("""
            using System;
            using System.Collections.Generic;
            using Probe;
            public enum State { Ready, Done }
            [Flags] public enum Access { None = 0, Read = 1, Write = 2, Admin = 4 }
            public class Source { public State State { get; set; } public Access Access { get; set; } public int[] Values { get; set; } }
            public class Destination { public string State { get; set; } public string Access { get; set; } public int[] Values { get; set; } }
            public static class Calls
            {
                public static Destination Map(Source source) => source.Adapt<Destination>();
                public static List<Destination> Map(IEnumerable<Source> source) => source.Adapt<List<Destination>>();
            }
            """);
        AssertNoErrors(output, diagnostics);
        foreach (var generated in run.Results.Single().GeneratedSources)
        {
            if (generated.HintName == "Adapt.ReflectionAdapter.g.cs") continue;
            var model = output.GetSemanticModel(generated.SyntaxTree);
            foreach (var invocation in generated.SyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var method = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                string? owner = method?.ContainingType.ToDisplayString();
                if (owner is "System.Type" or "System.Activator" or "System.Enum" ||
                    owner?.StartsWith("System.Reflection.", StringComparison.Ordinal) == true ||
                    owner?.StartsWith("System.Linq.Expressions.", StringComparison.Ordinal) == true)
                    throw new InvalidOperationException($"Runtime metadata/dynamic code call in {generated.HintName}: {invocation}");
            }
        }
    }

    [Test]
    public void Object_and_unrestricted_generic_calls_warn_and_keep_runtime_support()
    {
        foreach (string call in new[]
        {
            "public static Destination Map(object value) => value.Adapt<Destination>();",
            "public static Destination Map(object value) => value?.Adapt<Destination>();",
            "public static Destination Map(Source value) => value?.AdaptViaReflection<Destination>();",
            "public static TDest Map<TSource, TDest>(TSource value) where TSource : class where TDest : class => value?.Adapt<TDest>();",
            "public static TDest Map<TSource, TDest>(TSource value) => value.Adapt<TDest>();",
            "public static Destination Map(Source value) => value.AdaptViaReflection<Destination>();"
        })
        {
            var (output, diagnostics, run) = Generate("""
                using Probe;
                public class Source { public int Value { get; set; } }
                public class Destination { public int Value { get; set; } }
                """ + "public static class Calls { " + call + " }");
            AssertNoErrors(output, diagnostics);
            if (!diagnostics.Any(d => d.Id == "SGA005" && d.Severity == DiagnosticSeverity.Warning && d.Location.IsInSource))
                throw new InvalidOperationException("Missing fallback warning: " + call);
            if (!run.Results.Single().GeneratedSources.Any(s => s.HintName == "Adapt.ReflectionAdapter.g.cs"))
                throw new InvalidOperationException("Missing runtime fallback: " + call);
        }
    }

    [Test]
    public void Late_generated_calls_warn_instead_of_losing_runtime_support()
    {
        var (output, diagnostics, _) = Generate("public class Source { public int Value { get; set; } } public class Destination { public int Value { get; set; } }");
        var lateOutput = output.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using Probe;
            public static class LateCalls { public static Destination Map(Source value) => value.Adapt<Destination>(); }
            """));
        AssertNoErrors(lateOutput, diagnostics);
        if (!lateOutput.GetDiagnostics().Any(d => d.Id == "SGA005" && d.Severity == DiagnosticSeverity.Warning))
            throw new InvalidOperationException("Late-generated mapping did not warn.");
    }

    [Test]
    public void Generic_same_element_collection_conversion_does_not_warn()
    {
        var (output, diagnostics, _) = Generate("""
            using Probe;
            using System.Collections.Generic;
            public static class Calls { public static List<T> Map<T>(IEnumerable<T> values) => values.Adapt<List<T>, T>(); }
            """);
        AssertNoErrors(output, diagnostics);
        if (diagnostics.Concat(output.GetDiagnostics()).Any(d => d.Id == "SGA005"))
            throw new InvalidOperationException("Typed collection conversion unexpectedly warned.");
    }

    [Test]
    public void Invalid_destinations_report_generation_errors()
    {
        foreach (var destination in new[]
        {
            (Declaration: "public class Destination { public Destination(int value) {} public int Value { get; set; } }", Id: "SGA002"),
            (Declaration: "public class Destination { public int Other { get; set; } }", Id: "SGA003")
        })
        {
            var (_, diagnostics, _) = Generate("using Probe; public class Source { public int Value { get; set; } } " + destination.Declaration +
                " public static class Calls { public static Destination Map(Source value) => value.Adapt<Destination>(); }");
            if (!diagnostics.Any(d => d.Id == destination.Id && d.Severity == DiagnosticSeverity.Error))
                throw new InvalidOperationException("Missing diagnostic " + destination.Id);
        }
    }

    private static (Compilation Output, System.Collections.Immutable.ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult Run) Generate(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("Probe", [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AdaptGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        return (output, diagnostics, driver.GetRunResult());
    }

    private static void AssertNoErrors(Compilation output, IEnumerable<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Concat(output.GetDiagnostics()).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
    }
}
