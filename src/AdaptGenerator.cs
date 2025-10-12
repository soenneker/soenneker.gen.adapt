using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

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

        // Combine with compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)>)> compilationAndInvocations = 
            context.CompilationProvider.Combine(adaptInvocations.Collect());

        context.RegisterSourceOutput(compilationAndInvocations, static (spc, pack) =>
        {
            Compilation compilation = pack.Item1;
            ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations = pack.Item2;

            try
            {
                Emitter.Generate(spc, compilation, invocations);
            }
            catch (System.Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGA001", "AdaptGenerator error", ex.ToString(), "Adapt", DiagnosticSeverity.Warning, true),
                    Location.None));
            }
        });
    }
}
