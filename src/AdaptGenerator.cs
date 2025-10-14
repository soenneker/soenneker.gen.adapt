using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Soenneker.Gen.Adapt;

[Generator]
public sealed class AdaptGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all invocations - we'll filter for Adapt calls in the Emitter
        IncrementalValuesProvider<(InvocationExpressionSyntax, SemanticModel)> adaptInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is InvocationExpressionSyntax,
            static (ctx, _) => ((InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel));

        // Also scan .razor files for Adapt calls
        IncrementalValuesProvider<string> razorAdaptCalls = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".razor"))
            .Select(static (file, ct) =>
            {
                SourceText? text = file.GetText(ct);
                return text?.ToString() ?? string.Empty;
            })
            .SelectMany(static (content, _) => ExtractAdaptCallsFromRazor(content));

        // Combine everything with compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)>, ImmutableArray<string>)> allData = 
            context.CompilationProvider
                .Combine(adaptInvocations.Collect())
                .Combine(razorAdaptCalls.Collect())
                .Select(static (pair, _) => (pair.Left.Left, pair.Left.Right, pair.Right));

        context.RegisterSourceOutput(allData, static (spc, pack) =>
        {
            Compilation compilation = pack.Item1;
            ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations = pack.Item2;
            ImmutableArray<string> razorCalls = pack.Item3;

            try
            {
                Emitter.Generate(spc, compilation, invocations, razorCalls);
            }
            catch (System.Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGA001", "AdaptGenerator error", ex.ToString(), "Adapt", DiagnosticSeverity.Warning, true),
                    Location.None));
            }
        });
    }

    private static ImmutableArray<string> ExtractAdaptCallsFromRazor(string content)
    {
        var results = ImmutableArray.CreateBuilder<string>();
        
        // Regex to find .Adapt<TypeName>() patterns in Razor files
        // Matches patterns like: new SomeType().Adapt<TargetType>()
        var pattern = @"new\s+([a-zA-Z_][a-zA-Z0-9_\.]*)\s*\(\s*\)\s*\.\s*Adapt\s*<\s*([a-zA-Z_][a-zA-Z0-9_\.]*)\s*>\s*\(";
        var matches = Regex.Matches(content, pattern);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string sourceType = match.Groups[1].Value;
                string destType = match.Groups[2].Value;
                results.Add($"{sourceType}|{destType}");
            }
        }
        
        return results.ToImmutable();
    }
}
