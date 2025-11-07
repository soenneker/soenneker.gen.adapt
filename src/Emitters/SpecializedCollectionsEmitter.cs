using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class SpecializedCollectionsEmitter
{
    /// <summary>
    /// Emits specialized collection adaptations (HashSet, Queue, Stack, etc.)
    /// </summary>
    public static void EmitSpecializedCollections(StringBuilder sb)
    {
        // HashSet<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static HashSet<TElement> Adapt<TElement>(this HashSet<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new HashSet<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ISet<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ISet<TElement> Adapt<TElement>(this ISet<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new HashSet<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // SortedSet<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static SortedSet<TElement> Adapt<TElement>(this SortedSet<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new SortedSet<TElement>(source, source.Comparer);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // Queue<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static Queue<TElement> Adapt<TElement>(this Queue<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new Queue<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // Stack<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static Stack<TElement> Adapt<TElement>(this Stack<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new Stack<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // LinkedList<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static LinkedList<TElement> Adapt<TElement>(this LinkedList<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new LinkedList<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // Collection<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static Collection<TElement> Adapt<TElement>(this Collection<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new Collection<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ObservableCollection<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ObservableCollection<TElement> Adapt<TElement>(this ObservableCollection<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new ObservableCollection<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }
}
