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
            // Note: Can't use 'in' with property accesses or expressions, only with method parameters
            return "Map_" + fromSan + "_To_" + toSan + "(" + expr + ")";
        }

        bool fromIsDictionary = Types.IsAnyDictionary(fromType, out _, out _);
        bool toIsDictionary = Types.IsAnyDictionary(toType, out _, out _);
        bool fromIsList = Types.IsAnyList(fromType, out _);
        bool toIsList = Types.IsAnyList(toType, out _);
        bool fromIsEnumerable = Types.IsIEnumerable(fromType, out _);
        bool toIsEnumerable = Types.IsIEnumerable(toType, out _);
        bool fromIsArray = Types.IsArray(fromType, out _);
        bool toIsArray = Types.IsArray(toType, out _);

        if ((fromIsDictionary || fromIsList || fromIsEnumerable || fromIsArray) &&
            (toIsDictionary || toIsList || toIsEnumerable || toIsArray))
        {
            return "(" + expr + ").Adapt<" + Types.ShortName(toType) + ">()";
        }

        return "(" + Types.ShortName(toType) + ")" + expr;
    }

    private static string San(INamedTypeSymbol type)
    {
        return type.Name.Replace(".", "_").Replace("`", "_");
    }
}
