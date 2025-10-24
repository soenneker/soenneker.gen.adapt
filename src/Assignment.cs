using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Soenneker.Gen.Adapt;

internal static class Assignment
{
    /// <summary>Returns C# assignment expression or null if unsupported.</summary>
    public static string? TryBuild(string srcExpr, ITypeSymbol srcType, ITypeSymbol dstType, List<INamedTypeSymbol> enums)
    {
        // Same type (lists and dicts handled in mapper)
        if (SymbolEqualityComparer.Default.Equals(srcType, dstType))
            return srcExpr;

        // List-like collections handled by mapper
        if (Types.IsAnyList(srcType, out _) && Types.IsAnyList(dstType, out _))
            return null;

        // Dictionary-like collections handled by mapper
        if (Types.IsAnyDictionary(srcType, out _, out _) && Types.IsAnyDictionary(dstType, out _, out _))
            return null;

        // enum -> string
        if (srcType.TypeKind == TypeKind.Enum && Types.IsString(dstType))
            return srcExpr + ".ToString()";

        // string -> enum
        if (Types.IsString(srcType) && dstType.TypeKind == TypeKind.Enum)
            return "GenAdapt_EnumParsers.Parse_" + San((INamedTypeSymbol)dstType) + "(" + srcExpr + ")";

        // enum -> int
        if (srcType.TypeKind == TypeKind.Enum && Types.IsInt(dstType))
            return "(int)" + srcExpr;

        // int -> enum
        if (Types.IsInt(srcType) && dstType.TypeKind == TypeKind.Enum)
            return "(" + Types.Fq(dstType) + ")" + srcExpr;

        // Guid <-> string
        if (Types.IsGuid(srcType) && Types.IsString(dstType))
            return srcExpr + ".ToString()";

        if (Types.IsString(srcType) && Types.IsGuid(dstType))
            return null; // Handle Guid.TryParse specially in mapper

        // Nullable<T> handling (pass-through for identical inner)
        if (Types.IsNullableOf(srcType, out ITypeSymbol? sInner) && Types.IsNullableOf(dstType, out ITypeSymbol? dInner))
        {
            if (SymbolEqualityComparer.Default.Equals(sInner!, dInner!))
                return srcExpr; // same nullable type
        }

        // Intellenum-like: class with public int Value -> int
        if (srcType is INamedTypeSymbol cls1 && Types.HasIntValueProp(cls1) && Types.IsInt(dstType))
            return srcExpr + ".Value";

        // int -> class with static From(int)
        if (Types.IsInt(srcType) && dstType is INamedTypeSymbol cls2 && Types.HasStaticFromInt(cls2))
            return Types.Fq(cls2) + ".From(" + srcExpr + ")";

        // User-defined type -> user-defined type (call specific Map_* method within GenAdapt)
        if (srcType is INamedTypeSymbol sType &&
            dstType is INamedTypeSymbol dType &&
            (sType.TypeKind == TypeKind.Class || sType.TypeKind == TypeKind.Struct || sType.TypeKind == TypeKind.Interface) &&
            (dType.TypeKind == TypeKind.Class || dType.TypeKind == TypeKind.Struct) &&
            !Types.IsFrameworkType(sType) && !Types.IsFrameworkType(dType))
        {
            string sSan = San(sType);
            string dSan = San(dType);
            return "Map_" + sSan + "_To_" + dSan + "(" + srcExpr + ")";
        }

        return null;
    }

    /// <summary>Checks assignment feasibility without emitting code.</summary>
    public static bool CanAssign(ITypeSymbol srcType, ITypeSymbol dstType, List<INamedTypeSymbol> enums)
    {
        if (SymbolEqualityComparer.Default.Equals(srcType, dstType))
            return true;

        if (Types.IsAnyList(srcType, out _) && Types.IsAnyList(dstType, out _))
            return true;

        if (Types.IsAnyDictionary(srcType, out _, out _) && Types.IsAnyDictionary(dstType, out _, out _))
            return true;

        if (srcType.TypeKind == TypeKind.Enum && (Types.IsString(dstType) || Types.IsInt(dstType)))
            return true;
        if (Types.IsString(srcType) && dstType.TypeKind == TypeKind.Enum)
            return true;
        if (Types.IsInt(srcType) && dstType.TypeKind == TypeKind.Enum)
            return true;

        if (Types.IsGuid(srcType) && Types.IsString(dstType))
            return true;
        if (Types.IsString(srcType) && Types.IsGuid(dstType))
            return true;

        if (Types.IsNullableOf(srcType, out ITypeSymbol? sInner) && Types.IsNullableOf(dstType, out ITypeSymbol? dInner) &&
            SymbolEqualityComparer.Default.Equals(sInner!, dInner!))
            return true;

        if (srcType is INamedTypeSymbol cls1 && Types.HasIntValueProp(cls1) && Types.IsInt(dstType))
            return true;
        if (Types.IsInt(srcType) && dstType is INamedTypeSymbol cls2 && Types.HasStaticFromInt(cls2))
            return true;

        if (srcType is INamedTypeSymbol sType &&
            dstType is INamedTypeSymbol dType &&
            (sType.TypeKind == TypeKind.Class || sType.TypeKind == TypeKind.Struct || sType.TypeKind == TypeKind.Interface) &&
            (dType.TypeKind == TypeKind.Class || dType.TypeKind == TypeKind.Struct) &&
            !Types.IsFrameworkType(sType) && !Types.IsFrameworkType(dType))
            return true;

        return false;
    }

    private static string San(INamedTypeSymbol sym)
    {
        string s = sym.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            if ((ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= '0' && ch <= '9'))
                sb.Append(ch);
            else
                sb.Append('_');
        }
        return sb.ToString();
    }
}
