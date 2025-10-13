using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt;

internal static class Types
{
    public static bool IsList(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "List" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsIReadOnlyList(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "IReadOnlyList" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsIReadOnlyCollection(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "IReadOnlyCollection" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsIEnumerable(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "IEnumerable" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        
        // Also check for IEnumerable<T> with generic arity
        if (t is INamedTypeSymbol nt2 && nt2.Name == "IEnumerable`1" && nt2.TypeArguments.Length == 1)
        {
            elem = nt2.TypeArguments[0];
            return true;
        }
        
        return false;
    }

    public static bool IsArray(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is IArrayTypeSymbol arr)
        {
            elem = arr.ElementType;
            return true;
        }
        return false;
    }

    public static bool IsIList(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "IList" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsICollection(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "ICollection" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsHashSet(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "HashSet" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsISet(ITypeSymbol t, out ITypeSymbol? elem)
    {
        elem = null;
        if (t is INamedTypeSymbol nt && nt.Name == "ISet" && nt.TypeArguments.Length == 1)
        {
            elem = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static bool IsAnyList(ITypeSymbol t, out ITypeSymbol? elem)
    {
        return IsList(t, out elem) || 
               IsIReadOnlyList(t, out elem) || 
               IsIReadOnlyCollection(t, out elem) ||
               IsArray(t, out elem) ||
               IsIList(t, out elem) ||
               IsICollection(t, out elem) ||
               IsHashSet(t, out elem) ||
               IsISet(t, out elem);
    }

    public static bool IsDictionary(ITypeSymbol t, out ITypeSymbol? key, out ITypeSymbol? value)
    {
        key = null;
        value = null;
        if (t is INamedTypeSymbol nt && nt.Name == "Dictionary" && nt.TypeArguments.Length == 2)
        {
            key = nt.TypeArguments[0];
            value = nt.TypeArguments[1];
            return true;
        }
        return false;
    }

    public static bool IsIDictionary(ITypeSymbol t, out ITypeSymbol? key, out ITypeSymbol? value)
    {
        key = null;
        value = null;
        if (t is INamedTypeSymbol nt && nt.Name == "IDictionary" && nt.TypeArguments.Length == 2)
        {
            key = nt.TypeArguments[0];
            value = nt.TypeArguments[1];
            return true;
        }
        return false;
    }

    public static bool IsIReadOnlyDictionary(ITypeSymbol t, out ITypeSymbol? key, out ITypeSymbol? value)
    {
        key = null;
        value = null;
        if (t is INamedTypeSymbol nt && nt.Name == "IReadOnlyDictionary" && nt.TypeArguments.Length == 2)
        {
            key = nt.TypeArguments[0];
            value = nt.TypeArguments[1];
            return true;
        }
        return false;
    }

    public static bool IsAnyDictionary(ITypeSymbol t, out ITypeSymbol? key, out ITypeSymbol? value)
    {
        return IsDictionary(t, out key, out value) || 
               IsIDictionary(t, out key, out value) || 
               IsIReadOnlyDictionary(t, out key, out value);
    }

    public static bool IsString(ITypeSymbol t) => t.SpecialType == SpecialType.System_String;
    public static bool IsInt(ITypeSymbol t) => t.SpecialType == SpecialType.System_Int32;
    public static bool IsByte(ITypeSymbol t) => t.SpecialType == SpecialType.System_Byte;
    public static bool IsShort(ITypeSymbol t) => t.SpecialType == SpecialType.System_Int16;
    public static bool IsLong(ITypeSymbol t) => t.SpecialType == SpecialType.System_Int64;
    public static bool IsFloat(ITypeSymbol t) => t.SpecialType == SpecialType.System_Single;
    public static bool IsDouble(ITypeSymbol t) => t.SpecialType == SpecialType.System_Double;
    public static bool IsDecimal(ITypeSymbol t) => t.SpecialType == SpecialType.System_Decimal;
    public static bool IsBool(ITypeSymbol t) => t.SpecialType == SpecialType.System_Boolean;
    public static bool IsChar(ITypeSymbol t) => t.SpecialType == SpecialType.System_Char;
    public static bool IsSByte(ITypeSymbol t) => t.SpecialType == SpecialType.System_SByte;
    public static bool IsUShort(ITypeSymbol t) => t.SpecialType == SpecialType.System_UInt16;
    public static bool IsUInt(ITypeSymbol t) => t.SpecialType == SpecialType.System_UInt32;
    public static bool IsULong(ITypeSymbol t) => t.SpecialType == SpecialType.System_UInt64;

    public static bool IsGuid(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.Guid";
        }
        return false;
    }

    public static bool IsDateTime(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.DateTime";
        }
        return false;
    }

    public static bool IsDateTimeOffset(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.DateTimeOffset";
        }
        return false;
    }

    public static bool IsTimeSpan(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.TimeSpan";
        }
        return false;
    }

    public static bool IsDateOnly(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.DateOnly";
        }
        return false;
    }

    public static bool IsTimeOnly(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.TimeOnly";
        }
        return false;
    }

    public static bool IsUri(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.Uri";
        }
        return false;
    }

    public static bool IsNullableOf(ITypeSymbol t, out ITypeSymbol? inner)
    {
        inner = null;
        if (t is INamedTypeSymbol nt && nt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && nt.TypeArguments.Length == 1)
        {
            inner = nt.TypeArguments[0];
            return true;
        }
        return false;
    }

    public static string Fq(ITypeSymbol t) => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static List<string> GetEnumMemberNames(INamedTypeSymbol e)
    {
        var list = new List<string>(8);
        foreach (ISymbol? m in e.GetMembers())
            if (m is IFieldSymbol f && f.HasConstantValue)
                list.Add(f.Name);
        return list;
    }

    public static bool HasIntValueProp(INamedTypeSymbol t)
    {
        foreach (ISymbol? m in t.GetMembers())
        {
            if (m is IPropertySymbol p &&
                p.Name == "Value" &&
                p.Type.SpecialType == SpecialType.System_Int32 &&
                p.GetMethod is not null)
                return true;
        }
        return false;
    }

    public static bool HasStaticFromInt(INamedTypeSymbol t)
    {
        foreach (ISymbol? m in t.GetMembers())
        {
            if (m is IMethodSymbol me &&
                me.IsStatic &&
                me.Name == "From" &&
                me.Parameters.Length == 1 &&
                me.Parameters[0].Type.SpecialType == SpecialType.System_Int32 &&
                SymbolEqualityComparer.Default.Equals(me.ReturnType, t))
                return true;
        }
        return false;
    }

    public static bool IsFrameworkType(INamedTypeSymbol t)
    {
        string? ns = t.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (ns is null) return false;
        return ns == "global::System" || ns.StartsWith("global::System.");
    }
}
