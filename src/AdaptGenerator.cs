using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Soenneker.Gen.Adapt;

[Generator]
public sealed class AdaptGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> classSymbols = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node) as INamedTypeSymbol)
            .Where(static s => s is not null && s.TypeKind == TypeKind.Class)
            .Select(static (s, _) => s!)
            .Collect();

        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> structSymbols = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is StructDeclarationSyntax,
            static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node) as INamedTypeSymbol)
            .Where(static s => s is not null && s.TypeKind == TypeKind.Struct)
            .Select(static (s, _) => s!)
            .Collect();

        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> interfaceSymbols = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is InterfaceDeclarationSyntax,
            static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((InterfaceDeclarationSyntax)ctx.Node) as INamedTypeSymbol)
            .Where(static s => s is not null && s.TypeKind == TypeKind.Interface)
            .Select(static (s, _) => s!)
            .Collect();

        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> enumSymbols = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is EnumDeclarationSyntax,
            static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((EnumDeclarationSyntax)ctx.Node) as INamedTypeSymbol)
            .Where(static s => s is not null && s.TypeKind == TypeKind.Enum)
            .Select(static (s, _) => s!)
            .Collect();

        IncrementalValueProvider<((((Compilation Left, ImmutableArray<INamedTypeSymbol> Right) Left, ImmutableArray<INamedTypeSymbol> Right) Left, ImmutableArray<INamedTypeSymbol> Right) Left, ImmutableArray<INamedTypeSymbol> Right)> all = context.CompilationProvider.Combine(classSymbols).Combine(structSymbols).Combine(interfaceSymbols).Combine(enumSymbols);

        context.RegisterSourceOutput(all, static (spc, pack) =>
        {
            Compilation? compilation = pack.Left.Left.Left.Left;
            ImmutableArray<INamedTypeSymbol> classes = pack.Left.Left.Left.Right;
            ImmutableArray<INamedTypeSymbol> structs = pack.Left.Left.Right;
            ImmutableArray<INamedTypeSymbol> interfaces = pack.Left.Right;
            ImmutableArray<INamedTypeSymbol> enums = pack.Right;

            try
            {
                Emitter.Generate(spc, compilation, classes, structs, interfaces, enums);
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
