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
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]");
        sb.AppendLine("\t\tpublic static Dictionary<TKey, TValue> Adapt<TKey, TValue>(this Dictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar count = source.Count;");
        sb.AppendLine("\t\t\tif (count == 0) return new Dictionary<TKey, TValue>(0, source.Comparer);");
        sb.AppendLine("\t\t\tvar target = new Dictionary<TKey, TValue>(count, source.Comparer);");
        sb.AppendLine("\t\t\tforeach (var kv in source)");
        sb.AppendLine("\t\t\t{\n\t\t\t\tref var cell = ref global::System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _);\n\t\t\t\tcell = kv.Value;\n\t\t\t}");
        sb.AppendLine("\t\t\treturn target;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IDictionary<TKey,TValue>
        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]");
        sb.AppendLine("\t\tpublic static IDictionary<TKey, TValue> Adapt<TKey, TValue>(this IDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tif (source is Dictionary<TKey, TValue> dict)");
        sb.AppendLine("\t\t\t{\n\t\t\t\tvar count1 = dict.Count;\n\t\t\t\tif (count1 == 0) return new Dictionary<TKey, TValue>(0, dict.Comparer);\n\t\t\t\tvar target = new Dictionary<TKey, TValue>(count1, dict.Comparer);\n\t\t\t\tforeach (var kv in dict) { ref var cell = ref global::System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _); cell = kv.Value; }\n\t\t\t\treturn target;\n\t\t\t}");
        sb.AppendLine("\t\t\t{\n\t\t\t\tvar count2 = source.Count;\n\t\t\t\tif (count2 == 0) return new Dictionary<TKey, TValue>(0);\n\t\t\t\tvar target = new Dictionary<TKey, TValue>(count2);\n\t\t\t\tforeach (var kv in source) { ref var cell = ref global::System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _); cell = kv.Value; }\n\t\t\t\treturn target;\n\t\t\t}");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        // IReadOnlyDictionary<TKey,TValue>
        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]");
        sb.AppendLine("\t\tpublic static IReadOnlyDictionary<TKey, TValue> Adapt<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tif (source is Dictionary<TKey, TValue> dict)");
        sb.AppendLine("\t\t\t{\n\t\t\t\tvar count1 = dict.Count;\n\t\t\t\tif (count1 == 0) return new Dictionary<TKey, TValue>(0, dict.Comparer);\n\t\t\t\tvar target = new Dictionary<TKey, TValue>(count1, dict.Comparer);\n\t\t\t\tforeach (var kv in dict) { ref var cell = ref global::System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _); cell = kv.Value; }\n\t\t\t\treturn target;\n\t\t\t}");
        sb.AppendLine("\t\t\t{\n\t\t\t\tvar count2 = source.Count;\n\t\t\t\tif (count2 == 0) return new Dictionary<TKey, TValue>(0);\n\t\t\t\tvar target = new Dictionary<TKey, TValue>(count2);\n\t\t\t\tforeach (var kv in source) { ref var cell = ref global::System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _); cell = kv.Value; }\n\t\t\t\treturn target;\n\t\t\t}");
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
