using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class DictionaryCollectionsEmitter
{
    /// <summary>
    /// Emits dictionary collection adaptations
    /// </summary>
    public static void EmitDictionaryCollections(StringBuilder sb)
    {
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
        sb.AppendLine("\t\t\t{\n\t\t\t\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _);\n\t\t\t\tcell = kv.Value;\n\t\t\t}");
        sb.AppendLine("\t\t\treturn target;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]");
        sb.AppendLine("\t\tpublic static Dictionary<TKey, TValue> Adapt<TKey, TValue>(this IDictionary<TKey, TValue> source) where TKey : notnull");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (source is Dictionary<TKey, TValue> d)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tint count = d.Count;");
        sb.AppendLine("\t\t\t\tif (count == 0) return new Dictionary<TKey, TValue>(0, d.Comparer);");
        sb.AppendLine();
        sb.AppendLine("\t\t\t\tvar dest = new Dictionary<TKey, TValue>(count, d.Comparer);");
        sb.AppendLine("\t\t\t\tforeach (var kv in d)");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(dest, kv.Key, out _);");
        sb.AppendLine("\t\t\t\t\tcell = kv.Value;");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\treturn dest;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tint count = source.Count;");
        sb.AppendLine("\t\t\t\tif (count == 0) return new Dictionary<TKey, TValue>(0);");
        sb.AppendLine();
        sb.AppendLine("\t\t\t\tvar dest = new Dictionary<TKey, TValue>(count);");
        sb.AppendLine("\t\t\t\tforeach (var kv in source)");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(dest, kv.Key, out _);");
        sb.AppendLine("\t\t\t\t\tcell = kv.Value;");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\treturn dest;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t\t[GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]");
        sb.AppendLine("\t\tpublic static Dictionary<TKey, TValue> Adapt<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source) where TKey : notnull");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (source is Dictionary<TKey, TValue> d)");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tint count = d.Count;");
        sb.AppendLine("\t\t\t\tif (count == 0) return new Dictionary<TKey, TValue>(0, d.Comparer);");
        sb.AppendLine();
        sb.AppendLine("\t\t\t\tvar dest = new Dictionary<TKey, TValue>(count, d.Comparer);");
        sb.AppendLine("\t\t\t\tforeach (var kv in d)");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(dest, kv.Key, out _);");
        sb.AppendLine("\t\t\t\t\tcell = kv.Value;");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\treturn dest;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tint count = source.Count;");
        sb.AppendLine("\t\t\t\tif (count == 0) return new Dictionary<TKey, TValue>(0);");
        sb.AppendLine();
        sb.AppendLine("\t\t\t\tvar dest = new Dictionary<TKey, TValue>(count);");
        sb.AppendLine("\t\t\t\tforeach (var kv in source)");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(dest, kv.Key, out _);");
        sb.AppendLine("\t\t\t\t\tcell = kv.Value;");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t\treturn dest;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static SortedDictionary<TKey, TValue> Adapt<TKey, TValue>(this SortedDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new SortedDictionary<TKey, TValue>(source, source.Comparer);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ConcurrentDictionary<TKey, TValue> Adapt<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn new ConcurrentDictionary<TKey, TValue>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t\t[System.CodeDom.Compiler.GeneratedCode(\"Soenneker.Gen.Adapt\", \"3.0.0\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static ImmutableDictionary<TKey, TValue> Adapt<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\treturn source;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

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
