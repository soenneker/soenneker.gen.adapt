using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class DictionaryEmitter
{
    /// <summary>
    /// Handles dictionary-to-dictionary mapping logic.
    /// </summary>
    public static void EmitDictionaryMappingInstructions(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, ITypeSymbol sKey, ITypeSymbol sValue,
        ITypeSymbol dKey, ITypeSymbol dValue, NameCache names, string indent)
    {
        string dstFq = Types.Fq(dest);

        bool srcIsConcreteDict = Types.IsDictionary(source, out _, out _);
        bool srcIsIdict = Types.IsIDictionary(source, out _, out _);
        bool srcIsRoDict = Types.IsIReadOnlyDictionary(source, out _, out _);
        bool dstIsConcreteDict = Types.IsDictionary(dest, out _, out _);

        string sKeyFq = Types.ShortName(sKey);
        string sValFq = Types.ShortName(sValue);
        var dictOfSrc = $"Dictionary<{sKeyFq}, {sValFq}>";

        if (dstIsConcreteDict && SymbolEqualityComparer.Default.Equals(sKey, dKey) && SymbolEqualityComparer.Default.Equals(sValue, dValue))
        {
            if (srcIsConcreteDict)
            {
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

        sb.Append(indent).Append("var count4 = source.Count;").AppendLine();
        sb.Append(indent).Append("if (count4 == 0) return new ").Append(dstFq).AppendLine("(0);");
        sb.Append(indent).Append("var __result = new ").Append(dstFq).AppendLine("(count4);");
        sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
        sb.Append(indent).AppendLine("{");

        string keyExpr2 = SymbolEqualityComparer.Default.Equals(sKey, dKey) ? "kv.Key" : CollectionMappingHelper.GetConversionExpression("kv.Key", sKey, dKey, names);
        string valueExpr2 = SymbolEqualityComparer.Default.Equals(sValue, dValue) ? "kv.Value" : CollectionMappingHelper.GetConversionExpression("kv.Value", sValue, dValue, names);

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
}
