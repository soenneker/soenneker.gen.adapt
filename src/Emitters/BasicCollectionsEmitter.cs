using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class BasicCollectionsEmitter
{
    /// <summary>
    /// Emits basic collection adaptations (List, Array, IEnumerable, etc.)
    /// </summary>
    public static void EmitBasicCollections(StringBuilder sb)
    {
        // List<T> -> clone
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static List<TElement> Adapt<TElement>(this List<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar src = CollectionsMarshal.AsSpan(source);");
        sb.AppendLine("\t\t\tvar result = new List<TElement>(src.Length);");
        sb.AppendLine("\t\t\tCollectionsMarshal.SetCount(result, src.Length);");
        sb.AppendLine("\t\t\tsrc.CopyTo(CollectionsMarshal.AsSpan(result));");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IEnumerable<T> -> materialize (stable behavior)
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static IEnumerable<TElement> Adapt<TElement>(this IEnumerable<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tif (source is List<TElement> l)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tvar s = CollectionsMarshal.AsSpan(l);");
        sb.AppendLine("\t\t\t\tvar list = new List<TElement>(s.Length);");
        sb.AppendLine("\t\t\t\tCollectionsMarshal.SetCount(list, s.Length);");
        sb.AppendLine("\t\t\t\ts.CopyTo(CollectionsMarshal.AsSpan(list));");
        sb.AppendLine("\t\t\t\treturn list;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t\telse if (source is TElement[] a)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tvar list = new List<TElement>(a.Length);");
        sb.AppendLine("\t\t\t\tCollectionsMarshal.SetCount(list, a.Length);");
        sb.AppendLine("\t\t\t\ta.AsSpan().CopyTo(CollectionsMarshal.AsSpan(list));");
        sb.AppendLine("\t\t\t\treturn list;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t\telse");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tvar list = new List<TElement>();");
        sb.AppendLine("\t\t\t\tforeach (var item in source) list.Add(item);");
        sb.AppendLine("\t\t\t\treturn list;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // Array<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static TElement[] Adapt<TElement>(this TElement[] source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new TElement[source.Length];");
        sb.AppendLine("\t\t\tArray.Copy(source, result, source.Length);");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IList<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static IList<TElement> Adapt<TElement>(this IList<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new List<TElement>(source.Count);");
        sb.AppendLine("\t\t\tfor (int i = 0; i < source.Count; i++) result.Add(source[i]);");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ICollection<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ICollection<TElement> Adapt<TElement>(this ICollection<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new List<TElement>(source.Count);");
        sb.AppendLine("\t\t\tforeach (var item in source) result.Add(item);");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IReadOnlyList<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static IReadOnlyList<TElement> Adapt<TElement>(this IReadOnlyList<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new List<TElement>(source.Count);");
        sb.AppendLine("\t\t\tfor (int i = 0; i < source.Count; i++) result.Add(source[i]);");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IReadOnlyCollection<T>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static IReadOnlyCollection<TElement> Adapt<TElement>(this IReadOnlyCollection<TElement> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new List<TElement>(source.Count);");
        sb.AppendLine("\t\t\tforeach (var item in source) result.Add(item);");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }
}
