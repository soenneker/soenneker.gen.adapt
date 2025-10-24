using Microsoft.CodeAnalysis;
using System.Text;

namespace Soenneker.Gen.Adapt.Mappers;

internal static class CollectionMapper
{
    /// <summary>
    /// Handles collection-to-collection mapping logic
    /// </summary>
    public static void EmitListMappingInstructions(
        StringBuilder sb,
        INamedTypeSymbol source,
        INamedTypeSymbol dest,
        ITypeSymbol sElem,
        ITypeSymbol dElem,
        NameCache names,
        string indent)
    {
        string dstFq = names.FullyQualified(dest);
        
        bool srcIsList = Types.IsList(source, out _);
        bool srcIsArray = Types.IsArray(source, out _);
        bool srcIsROList = Types.IsIReadOnlyList(source, out _);
        bool srcIsIList = Types.IsIList(source, out _);

        if (srcIsList)
        {
            sb.Append(indent).Append("var src = CollectionsMarshal.AsSpan(source);").AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(src.Length);");
            sb.AppendLine();
            sb.Append(indent).Append("for (int i = 0; i < src.Length; i++)").AppendLine();
            sb.Append(indent).AppendLine("{");
            sb.Append(indent).Append("\tref readonly var s = ref src[i];").AppendLine();
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("\ttarget.Add(s);").AppendLine();
            }
            else
            {
                string itemExpr = GetConversionExpression("s", sElem, dElem, names);
                sb.Append(indent).Append("\ttarget.Add(").Append(itemExpr).AppendLine(");");
            }
            sb.Append(indent).AppendLine("}");
            sb.Append(indent).AppendLine("return target;");
        }
        else if (srcIsArray)
        {
            sb.Append(indent).Append("int n = source.Length;").AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(n);");
            sb.AppendLine();
            sb.Append(indent).Append("for (int i = 0; i < n; i++)").AppendLine();
            sb.Append(indent).AppendLine("{");
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("\ttarget.Add(source[i]);").AppendLine();
            }
            else
            {
                string itemExpr = GetConversionExpression("source[i]", sElem, dElem, names);
                sb.Append(indent).Append("\ttarget.Add(").Append(itemExpr).AppendLine(");");
            }
            sb.Append(indent).AppendLine("}");
            sb.Append(indent).AppendLine("return target;");
        }
        else if (srcIsROList || srcIsIList)
        {
            sb.Append(indent).Append("int count = source.Count;").AppendLine();
            sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(count);");
            sb.AppendLine();
            sb.Append(indent).Append("for (int i = 0; i < count; i++)").AppendLine();
            sb.Append(indent).AppendLine("{");
            if (SymbolEqualityComparer.Default.Equals(sElem, dElem))
            {
                sb.Append(indent).Append("\ttarget.Add(source[i]);").AppendLine();
            }
            else
            {
                string itemExpr = GetConversionExpression("source[i]", sElem, dElem, names);
                sb.Append(indent).Append("\ttarget.Add(").Append(itemExpr).AppendLine(");");
            }
            sb.Append(indent).AppendLine("}");
            sb.Append(indent).AppendLine("return target;");
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
            }
            
            sb.Append(indent).AppendLine("return target;");
        }
    }

    /// <summary>
    /// Handles dictionary-to-dictionary mapping logic
    /// </summary>
    public static void EmitDictionaryMappingInstructions(
        StringBuilder sb,
        INamedTypeSymbol source,
        INamedTypeSymbol dest,
        ITypeSymbol sKey,
        ITypeSymbol sValue,
        ITypeSymbol dKey,
        ITypeSymbol dValue,
        NameCache names,
        string indent)
    {
        string dstFq = names.FullyQualified(dest);
        
        sb.Append(indent).Append("var target = new ").Append(dstFq).AppendLine("(source.Count);");
        
        if (SymbolEqualityComparer.Default.Equals(sKey, dKey) && SymbolEqualityComparer.Default.Equals(sValue, dValue))
        {
            // Same types - direct copy
            sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
            sb.Append(indent).Append("\ttarget[kv.Key] = kv.Value;").AppendLine();
        }
        else
        {
            // Different types - need conversion
            sb.Append(indent).Append("foreach (var kv in source)").AppendLine();
            sb.Append(indent).AppendLine("{");
            
            string keyExpr = SymbolEqualityComparer.Default.Equals(sKey, dKey) 
                ? "kv.Key" 
                : GetConversionExpression("kv.Key", sKey, dKey, names);
                
            string valueExpr = SymbolEqualityComparer.Default.Equals(sValue, dValue) 
                ? "kv.Value" 
                : GetConversionExpression("kv.Value", sValue, dValue, names);
                
            sb.Append(indent).Append("\ttarget[").Append(keyExpr).Append("] = ").Append(valueExpr).AppendLine(";");
            sb.Append(indent).AppendLine("}");
        }
        
        sb.Append(indent).AppendLine("return target;");
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
            (toNamed.TypeKind == TypeKind.Class || toNamed.TypeKind == TypeKind.Struct) &&
            !Types.IsFrameworkType(fromNamed) && !Types.IsFrameworkType(toNamed))
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
