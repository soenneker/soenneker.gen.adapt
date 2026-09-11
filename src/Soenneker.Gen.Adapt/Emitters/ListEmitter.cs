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
        string dstType = Types.ShortName(dest);

        bool srcIsList = Types.IsList(source, out _);
        bool srcIsArray = Types.IsArray(source, out _);
        bool srcIsRoList = Types.IsIReadOnlyList(source, out _);
        bool srcIsIList = Types.IsIList(source, out _);
        bool destIsList = Types.IsList(dest, out _);

        if (!destIsList)
        {
            sb.Append(indent).Append("var target = new ").Append(dstType).AppendLine("();");
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

        if (source.TypeKind == TypeKind.Interface)
        {
            string elementType = Types.ShortName(sElem);
            // Only specialize the exact List type: a subclass can reimplement
            // IEnumerable/IList with different enumeration or indexing behavior.
            sb.Append(indent).Append("if (source.GetType() == typeof(List<").Append(elementType).AppendLine(">))");
            sb.Append(indent).AppendLine("{");
            EmitSpanMapping(sb, "CollectionsMarshal.AsSpan((List<" + elementType + ">)source)", dstType, sElem, dElem, names, indent + "\t");
            sb.Append(indent).AppendLine("}");
            sb.Append(indent).Append("if (source is ").Append(elementType).AppendLine("[] __sourceArray)");
            sb.Append(indent).AppendLine("{");
            EmitSpanMapping(sb, "new ReadOnlySpan<" + elementType + ">(__sourceArray)", dstType, sElem, dElem, names, indent + "\t");
            sb.Append(indent).AppendLine("}");
        }

        if (srcIsList)
        {
            sb.Append(indent).Append("var src = CollectionsMarshal.AsSpan(source);").AppendLine();
            sb.Append(indent).Append("int n = src.Length;").AppendLine();
            sb.AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstType).AppendLine("(n);");
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
            sb.Append(indent).Append("var target = new ").Append(dstType).AppendLine("(n);");
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
            sb.Append(indent).Append("var target = new ").Append(dstType).AppendLine("(count);");
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
            string elementType = Types.ShortName(sElem);
            sb.Append(indent).Append("int __capacity = source is IReadOnlyCollection<").Append(elementType)
                .AppendLine("> __readOnlyCollection ? __readOnlyCollection.Count : 0;");
            sb.Append(indent).AppendLine("if (__capacity == 0) System.Linq.Enumerable.TryGetNonEnumeratedCount(source, out __capacity);");
            sb.Append(indent).Append("var target = new ").Append(dstType).AppendLine("(__capacity);");
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

    private static void EmitSpanMapping(StringBuilder sb, string sourceExpression, string destinationType, ITypeSymbol sourceElement,
        ITypeSymbol destinationElement, NameCache names, string indent)
    {
        sb.Append(indent).Append("var __sourceSpan = ").Append(sourceExpression).AppendLine(";");
        sb.Append(indent).Append("var __spanResult = new ").Append(destinationType).AppendLine("(__sourceSpan.Length);");
        sb.Append(indent).AppendLine("CollectionsMarshal.SetCount(__spanResult, __sourceSpan.Length);");
        sb.Append(indent).AppendLine("var __destinationSpan = CollectionsMarshal.AsSpan(__spanResult);");
        if (SymbolEqualityComparer.Default.Equals(sourceElement, destinationElement))
        {
            sb.Append(indent).AppendLine("__sourceSpan.CopyTo(__destinationSpan);");
        }
        else
        {
            sb.Append(indent).AppendLine("for (int __index = 0; __index < __sourceSpan.Length; __index++)");
            sb.Append(indent).AppendLine("{");
            sb.Append(indent).AppendLine("\tref readonly var __item = ref __sourceSpan[__index];");
            sb.Append(indent).Append("\t__destinationSpan[__index] = ")
                .Append(CollectionMappingHelper.GetConversionExpression("__item", sourceElement, destinationElement, names)).AppendLine(";");
            sb.Append(indent).AppendLine("}");
        }
        sb.Append(indent).AppendLine("return __spanResult;");
    }
}
