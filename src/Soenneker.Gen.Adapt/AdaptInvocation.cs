using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Soenneker.Gen.Adapt;

internal static class AdaptInvocation
{
    internal static SimpleNameSyntax? GetName(InvocationExpressionSyntax call) => call.Expression switch
    {
        MemberAccessExpressionSyntax member => member.Name,
        MemberBindingExpressionSyntax binding => binding.Name,
        _ => null
    };

    internal static ExpressionSyntax? GetReceiver(InvocationExpressionSyntax call)
    {
        if (call.Expression is MemberAccessExpressionSyntax member)
            return member.Expression;
        if (call.Expression is MemberBindingExpressionSyntax)
        {
            for (SyntaxNode? node = call.Parent; node is not null; node = node.Parent)
                if (node is ConditionalAccessExpressionSyntax conditional)
                    return conditional.Expression;
        }
        return null;
    }
}
