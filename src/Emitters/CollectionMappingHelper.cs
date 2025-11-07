using Microsoft.CodeAnalysis;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class CollectionMappingHelper
{
    internal static string GetConversionExpression(string expr, ITypeSymbol fromType, ITypeSymbol toType, NameCache names)
    {
        if (SymbolEqualityComparer.Default.Equals(fromType, toType))
            return expr;

        if (fromType.TypeKind == TypeKind.Enum && Types.IsString(toType))
            return expr + ".ToString()";

        if (Types.IsString(fromType) && toType.TypeKind == TypeKind.Enum)
            return "GenAdapt_EnumParsers.Parse_" + San((INamedTypeSymbol)toType) + "(" + expr + ")";

        if (fromType.TypeKind == TypeKind.Enum && Types.IsInt(toType))
            return "(int)" + expr;

        if (Types.IsInt(fromType) && toType.TypeKind == TypeKind.Enum)
            return "(" + Types.Fq(toType) + ")" + expr;

        if (Types.IsGuid(fromType) && Types.IsString(toType))
            return expr + ".ToString()";

        if (Types.IsString(fromType) && Types.IsGuid(toType))
            return "global::System.Guid.TryParse(" + expr + ", out var g) ? g : default(global::System.Guid)";

        if (Types.IsNullableOf(fromType, out ITypeSymbol? fromInner) && Types.IsNullableOf(toType, out ITypeSymbol? toInner))
        {
            if (SymbolEqualityComparer.Default.Equals(fromInner!, toInner!))
                return expr;
        }

        if (fromType is INamedTypeSymbol fromNamed && toType is INamedTypeSymbol toNamed &&
            (fromNamed.TypeKind == TypeKind.Class || fromNamed.TypeKind == TypeKind.Struct) &&
            (toNamed.TypeKind == TypeKind.Class || toNamed.TypeKind == TypeKind.Struct) &&
            !Types.IsFrameworkType(fromNamed) && !Types.IsFrameworkType(toNamed))
        {
            string fromSan = names.Sanitized(fromNamed);
            string toSan = names.Sanitized(toNamed);
            if (fromNamed.TypeKind == TypeKind.Struct)
                return "GenAdapt.Map_" + fromSan + "_To_" + toSan + "(in " + expr + ")";
            return "GenAdapt.Map_" + fromSan + "_To_" + toSan + "(" + expr + ")";
        }

        return "(" + Types.Fq(toType) + ")" + expr;
    }

    private static string San(INamedTypeSymbol type)
    {
        return type.Name.Replace(".", "_").Replace("`", "_");
    }
}
