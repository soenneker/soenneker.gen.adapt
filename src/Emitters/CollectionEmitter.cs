using Microsoft.CodeAnalysis;
using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class CollectionEmitter
{
    /// <summary>
    /// Handles collection-to-collection mapping logic
    /// </summary>
    public static void EmitListMappingInstructions(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, ITypeSymbol sElem, ITypeSymbol dElem,
        NameCache names, string indent)
    {
        string dstFq = names.ShortName(dest);

        bool srcIsList = Types.IsList(source, out _);
        bool srcIsArray = Types.IsArray(source, out _);
        bool srcIsRoList = Types.IsIReadOnlyList(source, out _);
        bool srcIsIList = Types.IsIList(source, out _);

        if (srcIsList)
        {
            sb.Append(indent).Append("var src = CollectionsMarshal.AsSpan(source);").AppendLine();
            sb.Append(indent).Append("int n = src.Length;").AppendLine();
            sb.AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(n);");
            sb.AppendLine();
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("CollectionsMarshal.SetCount(target, n);").AppendLine();
                sb.Append(indent).Append("src.CopyTo(CollectionsMarshal.AsSpan(target));").AppendLine();
            }
            else
            {
                sb.Append(indent).Append("CollectionsMarshal.SetCount(target, n);").AppendLine();
                sb.Append(indent).Append("var targetSpan = CollectionsMarshal.AsSpan(target);").AppendLine();
                sb.Append(indent).Append("for (int i = 0; i < n; i++)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\tref readonly var s = ref src[i];").AppendLine();
                string itemExpr = GetConversionExpression("s", sElem, dElem, names);
                sb.Append(indent).Append("\ttargetSpan[i] = ").Append(itemExpr).AppendLine(";");
                sb.Append(indent).AppendLine("}");
            }
        }
        else if (srcIsArray)
        {
            sb.Append(indent).Append("int n = source.Length;").AppendLine();
            sb.AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(n);");
            sb.AppendLine();
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("CollectionsMarshal.SetCount(target, n);").AppendLine();
                sb.Append(indent).Append("source.AsSpan().CopyTo(CollectionsMarshal.AsSpan(target));").AppendLine();
            }
            else
            {
                sb.Append(indent).Append("CollectionsMarshal.SetCount(target, n);").AppendLine();
                sb.Append(indent).Append("var targetSpan = CollectionsMarshal.AsSpan(target);").AppendLine();
                sb.Append(indent).Append("for (int i = 0; i < n; i++)").AppendLine();
                sb.Append(indent).AppendLine("{");
                string itemExpr = GetConversionExpression("source[i]", sElem, dElem, names);
                sb.Append(indent).Append("\ttargetSpan[i] = ").Append(itemExpr).AppendLine(";");
                sb.Append(indent).AppendLine("}");
            }
        }
        else if (srcIsRoList || srcIsIList)
        {
            sb.Append(indent).Append("int count = source.Count;").AppendLine();
            sb.AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(count);");
            sb.AppendLine();
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("CollectionsMarshal.SetCount(target, count);").AppendLine();
                sb.Append(indent).Append("var targetSpan = CollectionsMarshal.AsSpan(target);").AppendLine();
                sb.Append(indent).Append("for (int i = 0; i < count; i++)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\ttargetSpan[i] = source[i];").AppendLine();
                sb.Append(indent).AppendLine("}");
            }
            else
            {
                sb.Append(indent).Append("CollectionsMarshal.SetCount(target, count);").AppendLine();
                sb.Append(indent).Append("var targetSpan = CollectionsMarshal.AsSpan(target);").AppendLine();
                sb.Append(indent).Append("for (int i = 0; i < count; i++)").AppendLine();
                sb.Append(indent).AppendLine("{");
                string itemExpr = GetConversionExpression("source[i]", sElem, dElem, names);
                sb.Append(indent).Append("\ttargetSpan[i] = ").Append(itemExpr).AppendLine(";");
                sb.Append(indent).AppendLine("}");
            }
        }
        else
        {
            // Fallback for non-indexable IEnumerable sources
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("();");

            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("foreach (var item in source)").AppendLine();
                sb.Append(indent).Append("\ttarget.Add(item);").AppendLine();
            }
            else
            {
                sb.Append(indent).Append("foreach (var item in source)").AppendLine();
                sb.Append(indent).AppendLine("{");
                string itemExpr = GetConversionExpression("item", sElem, dElem, names);
                sb.Append(indent).Append("\ttarget.Add(").Append(itemExpr).AppendLine(");");
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("return target;");
            }
        }

        sb.Append(indent).AppendLine("return target;");
    }

    /// <summary>
    /// Handles dictionary-to-dictionary mapping logic
    /// </summary>
    public static void EmitDictionaryMappingInstructions(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, ITypeSymbol sKey, ITypeSymbol sValue,
        ITypeSymbol dKey, ITypeSymbol dValue, NameCache names, string indent)
    {
        string dstFq = names.ShortName(dest);

        bool srcIsConcreteDict = Types.IsDictionary(source, out _, out _);
        bool srcIsIdict = Types.IsIDictionary(source, out _, out _);
        bool srcIsRoDict = Types.IsIReadOnlyDictionary(source, out _, out _);
        bool dstIsConcreteDict = Types.IsDictionary(dest, out _, out _);

        string sKeyFq = Types.ShortName(sKey);
        string sValFq = Types.ShortName(sValue);
        var dictOfSrc = $"Dictionary<{sKeyFq}, {sValFq}>";

        // When keys/values are identical and destination is a concrete Dictionary, we can do an optimized fast copy.
        if (dstIsConcreteDict && SymbolEqualityComparer.Default.Equals(sKey, dKey) && SymbolEqualityComparer.Default.Equals(sValue, dValue))
        {
            if (srcIsConcreteDict)
            {
                // Preserve comparer and use single-lookup writes
                sb.Append(indent).Append("var count = source.Count;").AppendLine();
                sb.Append(indent).Append("if (count == 0) return new ").Append(dstFq).AppendLine("(0, source.Comparer);");
                sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(count, source.Comparer);");
                sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent)
                    .Append("\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(target, kv.Key, out _);")
                    .AppendLine();
                sb.Append(indent).Append("\tcell = kv.Value;").AppendLine();
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("return target;");
            }
            else if (srcIsIdict || srcIsRoDict)
            {
                // Try to preserve comparer if the runtime source is actually Dictionary<,>
                sb.Append(indent).Append("if (source is ").Append(dictOfSrc).AppendLine(" d)");
                sb.Append(indent).AppendLine("{");
                sb.Append(indent).Append("\tvar count1 = d.Count;").AppendLine();
                sb.Append(indent).Append("\tif (count1 == 0) return new ").Append(dstFq).AppendLine("(0, d.Comparer);");
                sb.Append(indent).Append("\tvar __dictTarget = new ").Append(dstFq).AppendLine("(count1, d.Comparer);");
                sb.Append(indent).Append("\tforeach (var kv in d)").AppendLine();
                sb.Append(indent).AppendLine("\t{");
                sb.Append(indent)
                    .Append(
                        "\t\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(__dictTarget, kv.Key, out _);")
                    .AppendLine();
                sb.Append(indent).Append("\t\tcell = kv.Value;").AppendLine();
                sb.Append(indent).AppendLine("\t}");
                sb.Append(indent).AppendLine("\treturn __dictTarget;");
                sb.Append(indent).AppendLine("}");
                // Fallback without comparer
                sb.Append(indent).Append("var count2 = source.Count;").AppendLine();
                sb.Append(indent).Append("if (count2 == 0) return new ").Append(dstFq).AppendLine("(0);");
                sb.Append(indent).Append("var __idictFallback = new ").Append(dstFq).AppendLine("(count2);");
                sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent)
                    .Append(
                        "\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(__idictFallback, kv.Key, out _);")
                    .AppendLine();
                sb.Append(indent).Append("\tcell = kv.Value;").AppendLine();
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("return __idictFallback;");
            }
            else
            {
                // Unknown dictionary-like; fallback capacity and fast insert
                sb.Append(indent).Append("var count3 = source.Count;").AppendLine();
                sb.Append(indent).Append("if (count3 == 0) return new ").Append(dstFq).AppendLine("(0);");
                sb.Append(indent).Append("var __unknownTarget = new ").Append(dstFq).AppendLine("(count3);");
                sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
                sb.Append(indent).AppendLine("{");
                sb.Append(indent)
                    .Append(
                        "\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(__unknownTarget, kv.Key, out _);")
                    .AppendLine();
                sb.Append(indent).Append("\tcell = kv.Value;").AppendLine();
                sb.Append(indent).AppendLine("}");
                sb.Append(indent).AppendLine("return __unknownTarget;");
            }

            return;
        }

        // General path (possibly key/value conversion). We can still use single-lookup writes.
        sb.Append(indent).Append("var count4 = source.Count;").AppendLine();
        sb.Append(indent).Append("if (count4 == 0) return new ").Append(dstFq).AppendLine("(0);");
        sb.Append(indent).Append("var __result = new ").Append(dstFq).AppendLine("(count4);");
        sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
        sb.Append(indent).AppendLine("{");

        string keyExpr2 = SymbolEqualityComparer.Default.Equals(sKey, dKey) ? "kv.Key" : GetConversionExpression("kv.Key", sKey, dKey, names);

        string valueExpr2 = SymbolEqualityComparer.Default.Equals(sValue, dValue) ? "kv.Value" : GetConversionExpression("kv.Value", sValue, dValue, names);

        // Use a temporary for converted key to avoid double computation
        if (!SymbolEqualityComparer.Default.Equals(sKey, dKey))
        {
            sb.Append(indent).Append("\tvar __k = ").Append(keyExpr2).AppendLine(";");
            sb.Append(indent)
                .Append("\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(__result, __k, out _);")
                .AppendLine();
        }
        else
        {
            sb.Append(indent).Append("\tref var cell = ref CollectionsMarshal.GetValueRefOrAddDefault(__result, ")
                .Append(keyExpr2).Append(", out _);").AppendLine();
        }

        sb.Append(indent).Append("\tcell = ").Append(valueExpr2).AppendLine(";");
        sb.Append(indent).AppendLine("}");
        sb.Append(indent).AppendLine("return __result;");
    }

    private static string GetConversionExpression(string expr, ITypeSymbol fromType, ITypeSymbol toType, NameCache names)
    {
        // Handle basic type conversions
        if (SymbolEqualityComparer.Default.Equals(fromType, toType))
            return expr;

        // Handle enum conversions
        if (fromType.TypeKind == TypeKind.Enum && Types.IsString(toType))
            return expr + ".ToString()";

        if (Types.IsString(fromType) && toType.TypeKind == TypeKind.Enum)
            return "GenAdapt_EnumParsers.Parse_" + San((INamedTypeSymbol)toType) + "(" + expr + ")";

        if (fromType.TypeKind == TypeKind.Enum && Types.IsInt(toType))
            return "(int)" + expr;

        if (Types.IsInt(fromType) && toType.TypeKind == TypeKind.Enum)
            return "(" + Types.Fq(toType) + ")" + expr;

        // Handle Guid conversions
        if (Types.IsGuid(fromType) && Types.IsString(toType))
            return expr + ".ToString()";

        if (Types.IsString(fromType) && Types.IsGuid(toType))
            return "global::System.Guid.TryParse(" + expr + ", out var g) ? g : default(global::System.Guid)";

        // Handle nullable conversions
        if (Types.IsNullableOf(fromType, out ITypeSymbol? fromInner) && Types.IsNullableOf(toType, out ITypeSymbol? toInner))
        {
            if (SymbolEqualityComparer.Default.Equals(fromInner!, toInner!))
                return expr; // same nullable type
        }

        // Handle user-defined type conversions
        if (fromType is INamedTypeSymbol fromNamed && toType is INamedTypeSymbol toNamed &&
            (fromNamed.TypeKind == TypeKind.Class || fromNamed.TypeKind == TypeKind.Struct) &&
            (toNamed.TypeKind == TypeKind.Class || toNamed.TypeKind == TypeKind.Struct) && !Types.IsFrameworkType(fromNamed) && !Types.IsFrameworkType(toNamed))
        {
            string fromSan = names.Sanitized(fromNamed);
            string toSan = names.Sanitized(toNamed);
            if (fromNamed.TypeKind == TypeKind.Struct)
                return "Map_" + fromSan + "_To_" + toSan + "(in " + expr + ")";
            return "Map_" + fromSan + "_To_" + toSan + "(" + expr + ")";
        }

        // Default: cast
        return "(" + Types.Fq(toType) + ")" + expr;
    }

    private static string San(INamedTypeSymbol type)
    {
        // Simple sanitization for enum parser names
        return type.Name.Replace(".", "_").Replace("`", "_");
    }
}