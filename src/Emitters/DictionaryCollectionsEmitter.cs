using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class DictionaryCollectionsEmitter
{
    /// <summary>
    /// Emits dictionary collection adaptations
    /// </summary>
    public static void EmitDictionaryCollections(StringBuilder sb)
    {
        // Dictionary<TKey,TValue>
        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static Dictionary<TKey, TValue> Adapt<TKey, TValue>(this Dictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tif (source.Count == 0) return new Dictionary<TKey, TValue>();");
        sb.AppendLine("\t\t\tvar result = new Dictionary<TKey, TValue>(source.Count);");
        sb.AppendLine("\t\t\tforeach (var kv in source) result[kv.Key] = kv.Value;");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IDictionary<TKey,TValue>
        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static IDictionary<TKey, TValue> Adapt<TKey, TValue>(this IDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new Dictionary<TKey, TValue>(source.Count);");
        sb.AppendLine("\t\t\tforeach (var kv in source) result[kv.Key] = kv.Value;");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IReadOnlyDictionary<TKey,TValue>
        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static IReadOnlyDictionary<TKey, TValue> Adapt<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar result = new Dictionary<TKey, TValue>(source.Count);");
        sb.AppendLine("\t\t\tforeach (var kv in source) result[kv.Key] = kv.Value;");
        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // SortedDictionary<TKey,TValue>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static SortedDictionary<TKey, TValue> Adapt<TKey, TValue>(this SortedDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new SortedDictionary<TKey, TValue>(source, source.Comparer);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ConcurrentDictionary<TKey,TValue>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ConcurrentDictionary<TKey, TValue> Adapt<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new ConcurrentDictionary<TKey, TValue>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableDictionary<TKey,TValue>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableDictionary<TKey, TValue> Adapt<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // ImmutableSortedDictionary<TKey,TValue>
        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableSortedDictionary<TKey, TValue> Adapt<TKey, TValue>(this ImmutableSortedDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }
}
