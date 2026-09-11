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
        sb.AppendLine("\t\t\tSystem.Type? destElemType = destType.IsArray ? destType.GetElementType() : (destType.IsGenericType ? destType.GetGenericArguments()[0] : null);");
        sb.AppendLine("\t\t\tif (destElemType is null) throw new NotSupportedException(\"Unsupported Adapt target type: \" + destType.FullName);");
        sb.AppendLine();
        sb.AppendLine("\t\t\tvar listType = typeof(List<>).MakeGenericType(destElemType);");
        sb.AppendLine("\t\t\tvar list = (System.Collections.IList)System.Activator.CreateInstance(listType)!;");
        sb.AppendLine();
        sb.AppendLine("\t\t\tbool sameElem = destElemType == typeof(TSrc);");
        sb.AppendLine("\t\t\tif (sameElem)");
        sb.AppendLine("\t\t\t{ foreach (var item in source) list.Add(item!); }");
        sb.AppendLine("\t\t\telse");
        sb.AppendLine("\t\t\t{");
        sb.AppendLine("\t\t\t\tvar genAdaptMethods = typeof(GenAdapt).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);");
        sb.AppendLine("\t\t\t\tSystem.Reflection.MethodInfo? adaptViaReflection = typeof(GenAdapt).GetMethod(\"AdaptViaReflection\");");
        sb.AppendLine("\t\t\t\tSystem.Reflection.MethodInfo? adaptGenericForElem = null;");
        sb.AppendLine("\t\t\t\tforeach (var m in genAdaptMethods)");
        sb.AppendLine("\t\t\t\t{ if (m.Name == \"Adapt\" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(TSrc)) { adaptGenericForElem = m; break; } }");
        sb.AppendLine("\t\t\t\tvar elementAdapter = (adaptGenericForElem ?? adaptViaReflection!).MakeGenericMethod(destElemType);");
        sb.AppendLine("\t\t\t\tforeach (var item in source)");
        sb.AppendLine("\t\t\t\t{");
        sb.AppendLine("\t\t\t\t\tobject? converted = elementAdapter.Invoke(null, new object?[] { item! });");
        sb.AppendLine("\t\t\t\t\tlist.Add(converted!);");
        sb.AppendLine("\t\t\t\t}");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\tif (destType.IsArray)");
        sb.AppendLine("\t\t\t{ var arr = System.Array.CreateInstance(destElemType, list.Count); for (int i=0;i<list.Count;i++) arr.SetValue(list[i], i); return (TDest)(object)arr; }");
        sb.AppendLine("\t\t\tvar def = destType.IsGenericType ? destType.GetGenericTypeDefinition() : destType;");
        sb.AppendLine("\t\t\tif (def == typeof(List<>)) return (TDest)(object)list;");
        sb.AppendLine("\t\t\tif (def == typeof(HashSet<>)) { var hs = System.Activator.CreateInstance(destType, list)!; return (TDest)hs; }");
        sb.AppendLine("\t\t\tif (def == typeof(SortedSet<>)) { var ss = System.Activator.CreateInstance(destType, list)!; return (TDest)ss; }");
        sb.AppendLine("\t\t\tif (def == typeof(LinkedList<>)) { var ll = System.Activator.CreateInstance(destType, list)!; return (TDest)ll; }");
        sb.AppendLine("\t\t\tif (def == typeof(Queue<>)) { var q = System.Activator.CreateInstance(destType, list)!; return (TDest)q; }");
        sb.AppendLine("\t\t\tif (def == typeof(Stack<>)) { var st = System.Activator.CreateInstance(destType, list)!; return (TDest)st; }");
        sb.AppendLine("\t\t\tif (def.FullName == \"System.Collections.ObjectModel.Collection`1\") { var c = System.Activator.CreateInstance(destType, list)!; return (TDest)c; }");
        sb.AppendLine("\t\t\tif (def.FullName == \"System.Collections.ObjectModel.ObservableCollection`1\") { var oc = System.Activator.CreateInstance(destType, list)!; return (TDest)oc; }");
        sb.AppendLine("\t\t\tif (destType.IsAssignableFrom(listType)) return (TDest)(object)list;");
        sb.AppendLine("\t\t\tthrow new NotSupportedException(\"Unsupported Adapt target type: \" + destType.FullName);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();

        sb.AppendLine("\t}");
        sb.AppendLine("}");
    }
}
