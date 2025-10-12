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

    public static bool IsString(ITypeSymbol t) => t.SpecialType == SpecialType.System_String;
    public static bool IsInt(ITypeSymbol t) => t.SpecialType == SpecialType.System_Int32;

    public static bool IsGuid(ITypeSymbol t)
    {
        if (t is INamedTypeSymbol nt)
        {
            // Compare fully-qualified name to avoid alias issues
            string fq = nt.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return fq == "global::System.Guid";
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
        // Treat anything under global::System.* as framework (blocks user-type mapping for BCL)
        return ns == "global::System" || ns.StartsWith("global::System.");
    }
}
