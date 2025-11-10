using Microsoft.CodeAnalysis;
using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class ListEmitter
{
    /// <summary>
    /// Handles collection-to-collection mapping logic.
    /// </summary>
    public static void EmitListMappingInstructions(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, ITypeSymbol sElem, ITypeSymbol dElem,
        NameCache names, string indent)
    {
        string dstFq = Types.Fq(dest);

        bool srcIsList = Types.IsList(source, out _);
        bool srcIsArray = Types.IsArray(source, out _);
        bool srcIsRoList = Types.IsIReadOnlyList(source, out _);
        bool srcIsIList = Types.IsIList(source, out _);
        bool destIsList = Types.IsList(dest, out _);

        if (!destIsList)
        {
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("();");
            sb.Append(indent).Append("foreach (var item in source)").AppendLine();
            sb.Append(indent).AppendLine("{");
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("\ttarget.Add(item);").AppendLine();
            }
            else
            {
                string itemExprFallback = CollectionMappingHelper.GetConversionExpression("item", sElem, dElem, names);
                sb.Append(indent).Append("\ttarget.Add(").Append(itemExprFallback).AppendLine(");");
            }

            sb.Append(indent).AppendLine("}");
            sb.Append(indent).AppendLine("return target;");
            return;
        }

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
                string itemExpr = CollectionMappingHelper.GetConversionExpression("s", sElem, dElem, names);
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
                string itemExpr = CollectionMappingHelper.GetConversionExpression("source[i]", sElem, dElem, names);
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
                string itemExpr = CollectionMappingHelper.GetConversionExpression("source[i]", sElem, dElem, names);
                sb.Append(indent).Append("\ttargetSpan[i] = ").Append(itemExpr).AppendLine(";");
                sb.Append(indent).AppendLine("}");
            }
        }
        else
        {
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("();");
            sb.Append(indent).Append("foreach (var item in source)").AppendLine();
            sb.Append(indent).AppendLine("{");
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("\ttarget.Add(item);").AppendLine();
            }
            else
            {
                string itemExpr = CollectionMappingHelper.GetConversionExpression("item", sElem, dElem, names);
                sb.Append(indent).Append("\ttarget.Add(").Append(itemExpr).AppendLine(");");
            }

            sb.Append(indent).AppendLine("}");
            sb.Append(indent).AppendLine("return target;");
            return;
        }

        sb.Append(indent).AppendLine("return target;");
    }
}