using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class ConcurrentCollectionsEmitter
{
    /// <summary>
    /// Emits concurrent collection adaptations
    /// </summary>
    public static void EmitConcurrentCollections(StringBuilder sb)
    {
        // ConcurrentBag<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ConcurrentBag<TElement> Adapt<TElement>(this ConcurrentBag<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new ConcurrentBag<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ConcurrentQueue<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ConcurrentQueue<TElement> Adapt<TElement>(this ConcurrentQueue<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new ConcurrentQueue<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ConcurrentStack<T>
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ConcurrentStack<TElement> Adapt<TElement>(this ConcurrentStack<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new ConcurrentStack<TElement>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }
}
