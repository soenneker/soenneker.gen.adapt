using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class CollectionsEmitter
{
    public static void Emit(StringBuilder sb, string targetNamespace)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Collections.ObjectModel;");
        sb.AppendLine("using System.Collections.Concurrent;");
        sb.AppendLine("using System.Collections.Immutable;");
        sb.AppendLine("using System.Runtime.InteropServices;");
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine();
        sb.Append("namespace ").AppendLine(targetNamespace);
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static partial class GenAdapt");
        sb.AppendLine("\t{");
        
        // Emit different collection types from separate files
        BasicCollectionsEmitter.EmitBasicCollections(sb);
        DictionaryCollectionsEmitter.EmitDictionaryCollections(sb);
        SpecializedCollectionsEmitter.EmitSpecializedCollections(sb);
        ConcurrentCollectionsEmitter.EmitConcurrentCollections(sb);
        ImmutableCollectionsEmitter.EmitImmutableCollections(sb);

        // Generic cross-shape adapter for any IEnumerable<TSrc> to destination collection TDest
        sb.AppendLine($"\t\t[System.CodeDom.Compiler.GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("\t\tpublic static TDest Adapt<TDest, TSrc>(this IEnumerable<TSrc> source)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tif (source is null) throw new ArgumentNullException(nameof(source));");
        sb.AppendLine("\t\t\tvar destType = typeof(TDest);");
        // These branches become constants for closed generic calls. Keep value-type
        // elements in typed collections instead of boxing through IList/reflection.
        sb.AppendLine("\t\t\tif (destType == typeof(TSrc[])) return (TDest)(object)System.Linq.Enumerable.ToArray(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(List<TSrc>) || destType == typeof(IEnumerable<TSrc>) || destType == typeof(IList<TSrc>) || destType == typeof(ICollection<TSrc>) || destType == typeof(IReadOnlyList<TSrc>) || destType == typeof(IReadOnlyCollection<TSrc>)) return (TDest)(object)new List<TSrc>(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(HashSet<TSrc>)) return (TDest)(object)new HashSet<TSrc>(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(SortedSet<TSrc>)) return (TDest)(object)new SortedSet<TSrc>(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(LinkedList<TSrc>)) return (TDest)(object)new LinkedList<TSrc>(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(Queue<TSrc>)) return (TDest)(object)new Queue<TSrc>(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(Stack<TSrc>)) return (TDest)(object)new Stack<TSrc>(source);");
        sb.AppendLine("\t\t\tif (destType == typeof(Collection<TSrc>)) return (TDest)(object)new Collection<TSrc>(new List<TSrc>(source));");
        sb.AppendLine("\t\t\tif (destType == typeof(ObservableCollection<TSrc>)) return (TDest)(object)new ObservableCollection<TSrc>(source);");
        sb.AppendLine("\t\t\treturn AdaptViaReflection<TDest>(source);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t}");
        sb.AppendLine("}");
    }
}
