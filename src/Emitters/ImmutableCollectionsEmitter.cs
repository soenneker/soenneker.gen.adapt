using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class ImmutableCollectionsEmitter
{
    /// <summary>
    /// Emits immutable collection adaptations
    /// </summary>
    public static void EmitImmutableCollections(StringBuilder sb)
    {
        // ImmutableArray<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableArray<TElement> Adapt<TElement>(this ImmutableArray<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source.IsDefault) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableList<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableList<TElement> Adapt<TElement>(this ImmutableList<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableHashSet<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableHashSet<TElement> Adapt<TElement>(this ImmutableHashSet<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableSortedSet<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableSortedSet<TElement> Adapt<TElement>(this ImmutableSortedSet<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableQueue<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableQueue<TElement> Adapt<TElement>(this ImmutableQueue<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source.IsEmpty) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableStack<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableStack<TElement> Adapt<TElement>(this ImmutableStack<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source.IsEmpty) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }
}
