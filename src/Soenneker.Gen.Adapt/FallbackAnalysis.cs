using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Soenneker.Gen.Adapt;

internal static class FallbackAnalysis
{
    internal static readonly DiagnosticDescriptor Warning = new("SGA005", "Runtime mapping fallback",
        "This Adapt call requires runtime mapping because {0}. Prefer concrete source and destination types for reflection-free generated mapping.",
        "Adapt", DiagnosticSeverity.Warning, true);

    internal static bool IsOpen(ITypeSymbol? type)
    {
        if (type is ITypeParameterSymbol || type?.TypeKind == TypeKind.Dynamic)
            return true;
        if (type is IArrayTypeSymbol array)
            return IsOpen(array.ElementType);
        if (type is INamedTypeSymbol named)
        {
            foreach (ITypeSymbol argument in named.TypeArguments)
                if (IsOpen(argument)) return true;
            return named.ContainingType is not null && IsOpen(named.ContainingType);
        }
        return false;
    }

    internal static string? Reason(SemanticModel model, InvocationExpressionSyntax call)
    {
        if (call.Expression is not MemberAccessExpressionSyntax { Name: GenericNameSyntax name } member)
            return null;
        if (model.GetSymbolInfo(call).Symbol is IMethodSymbol { ContainingType: { } owner } && owner.ToDisplayString() == "Mapster.TypeAdapter")
            return null;
        if (name.Identifier.ValueText == "AdaptViaReflection")
            return "AdaptViaReflection was explicitly requested";
        if (name.Identifier.ValueText != "Adapt") return null;
        ITypeSymbol? source = model.GetTypeInfo(member.Expression).Type;
        ITypeSymbol? destination = model.GetTypeInfo(name.TypeArgumentList.Arguments[0]).Type;
        if (name.TypeArgumentList.Arguments.Count == 2)
        {
            ITypeSymbol? element = model.GetTypeInfo(name.TypeArgumentList.Arguments[1]).Type;
            ITypeSymbol? destinationElement = destination is IArrayTypeSymbol array ? array.ElementType :
                destination is INamedTypeSymbol { TypeArguments.Length: 1 } named ? named.TypeArguments[0] : null;
            if (!SymbolEqualityComparer.Default.Equals(element, destinationElement))
                return "the two-argument collection adapter requires runtime element conversion; use a concrete Adapt<TDestination>() call";
            return null;
        }
        if (IsOpen(source) || IsOpen(destination)) return "the source or destination contains an open generic type parameter";
        if (source?.SpecialType == SpecialType.System_Object) return "the source is typed as object";
        return null;
    }
}
