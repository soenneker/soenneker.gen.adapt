using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class MappingEmitter
{
    public static void EmitSourceMapperAndDispatcher(StringBuilder sb, INamedTypeSymbol source, List<INamedTypeSymbol> destinations,
        List<INamedTypeSymbol> enums, NameCache names, string targetNamespace, Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>? referencedPairs = null)
    {
        string srcType = Types.ShortName(source);
        string srcSan = names.Sanitized(source);

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Collections.ObjectModel;");
        sb.AppendLine("using System.Collections.Concurrent;");
        sb.AppendLine("using System.Collections.Immutable;");
        sb.AppendLine("using System.Runtime.InteropServices;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine();
        sb.Append("namespace ").AppendLine(targetNamespace);
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static partial class GenAdapt");
        sb.AppendLine("\t{");

        if (destinations.Count == 1)
        {
            // Single destination: for collection sources emit Adapt(source); for object sources emit Map_* for nested calls
            INamedTypeSymbol d = destinations[0];
            string dType = Types.ShortName(d);
            bool sourceIsList = Types.IsAnyList(source, out _);
            bool sourceIsDict = Types.IsAnyDictionary(source, out _, out _);
            bool sourceIsIEnum = Types.IsIEnumerable(source, out _);
            bool srcIsStruct = source.TypeKind == TypeKind.Struct;

            sb.AppendLine($"\t\t[GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
            sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
            sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            if (sourceIsList || sourceIsDict || sourceIsIEnum)
            {
                // Private non-generic method: Adapt(source) for collection mapping rename
                sb.Append("\t\tprivate static ").Append(dType).Append(" Adapt(");
                if (srcIsStruct)
                    sb.Append("in ");
                sb.Append(srcType).AppendLine(" source)");
            }
            else
            {
                // Private Map_* for object mapping to support nested calls
                string srcSanLocal0 = names.Sanitized(source);
                string dstSanLocal0 = names.Sanitized(d);
                sb.Append("\t\tinternal static ").Append(dType).Append(" Map_").Append(srcSanLocal0).Append("_To_").Append(dstSanLocal0).Append('(');
                if (srcIsStruct)
                    sb.Append("in ");
                sb.Append(srcType).AppendLine(" source)");
            }

            sb.AppendLine("\t\t{");
            EmitMappingBody(sb, source, d, enums, names, "\t\t\t");
            sb.AppendLine("\t\t}");
            sb.AppendLine();

            // Generic overload (for explicit .Adapt<TDest>() calls) with Unsafe.As
            sb.AppendLine($"\t\t[GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
            sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
            sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("\t\tpublic static TDest Adapt<TDest>(this ").Append(srcType).AppendLine(" source)");
            sb.AppendLine("\t\t{");
            if (sourceIsList || sourceIsDict || sourceIsIEnum)
            {
                sb.AppendLine("\t\t\tvar r = Adapt(source);");
            }
            else
            {
                string srcSanLocal1 = names.Sanitized(source);
                string dstSanLocal1 = names.Sanitized(d);
                if (srcIsStruct)
                    sb.Append("\t\t\tvar r = Map_").Append(srcSanLocal1).Append("_To_").Append(dstSanLocal1).AppendLine("(in source);");
                else
                    sb.Append("\t\t\tvar r = Map_").Append(srcSanLocal1).Append("_To_").Append(dstSanLocal1).AppendLine("(source);");
            }

            sb.Append("\t\t\treturn Unsafe.As<").Append(dType).Append(", TDest>(ref r);").AppendLine();
            sb.AppendLine("\t\t}");
            sb.AppendLine();

            // No per-call throw helper emitted here; __ThrowUnsupported_* is used by the function pointer
        }
        else
        {
            // Multiple destinations: emit private mapping methods + per-TDest id cache with switch dispatch
            for (var i = 0; i < destinations.Count; i++)
                EmitMappingMethod(sb, source, destinations[i], enums, names);

            // Generic overload using typeof-dispatch and Unsafe.As
            sb.AppendLine($"\t\t[GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
            sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
            sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("\t\tpublic static TDest Adapt<TDest>(this ").Append(srcType).AppendLine(" source)");
            sb.AppendLine("\t\t{");
            for (var i = 0; i < destinations.Count; i++)
            {
                INamedTypeSymbol? d = destinations[i];
                string dType = Types.ShortName(d);
                string dSan = names.Sanitized(d);
                sb.Append("\t\t\tif (typeof(TDest) == typeof(").Append(dType).AppendLine("))");
                sb.AppendLine("\t\t\t{");
                if (source.TypeKind == TypeKind.Struct)
                    sb.Append("\t\t\t\tvar r = Map_").Append(srcSan).Append("_To_").Append(dSan).AppendLine("(in source);");
                else
                    sb.Append("\t\t\t\tvar r = Map_").Append(srcSan).Append("_To_").Append(dSan).AppendLine("(source);");
                sb.Append("\t\t\t\treturn Unsafe.As<").Append(dType).Append(", TDest>(ref r);").AppendLine();
                sb.AppendLine("\t\t\t}");
            }

            sb.AppendLine("\t\t\tthrow new NotSupportedException(\"Unsupported Adapt target type: \" + typeof(TDest).FullName);");
            sb.AppendLine("\t\t}");
            sb.AppendLine();

            // (removed) Bulk path for List<source> since not used

            // No per-call throw helper emitted here; __ThrowUnsupported_* is used by the function pointer
        }

        sb.AppendLine("\t}");
        sb.AppendLine("}");
    }

    private static bool IsBaseClass(INamedTypeSymbol derived, INamedTypeSymbol potentialBase)
    {
        INamedTypeSymbol? current = derived.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, potentialBase))
                return true;
            current = current.BaseType;
        }

        return false;
    }

    private static void EmitMappingMethod(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, List<INamedTypeSymbol> enums, NameCache names)
    {
        var srcProps = TypeProps.Build(source);
        var dstProps = TypeProps.Build(dest);

        bool sourceIsList = Types.IsAnyList(source, out _);
        bool destIsList = Types.IsAnyList(dest, out _);
        bool sourceIsDict = Types.IsAnyDictionary(source, out _, out _);
        bool destIsDict = Types.IsAnyDictionary(dest, out _, out _);
        bool sourceIsArray = Types.IsArray(source, out _);
        bool destIsArray = Types.IsArray(dest, out _);
        bool sourceIsEnumerable = Types.IsIEnumerable(source, out _);

        if (dstProps.Settable.Count == 0 && !sourceIsList && !destIsList && !sourceIsDict && !destIsDict && !sourceIsArray && !destIsArray &&
            !sourceIsEnumerable)
            return;

        string srcType = Types.ShortName(source);
        string dstType = Types.ShortName(dest);
        string srcSan = names.Sanitized(source);
        string dstSan = names.Sanitized(dest);

        sb.AppendLine($"\t\t[GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
        sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
        sb.AppendLine("\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        bool _srcIsStruct2 = source.TypeKind == TypeKind.Struct;
        sb.Append("\t\tinternal static ").Append(dstType).Append(" Map_").Append(srcSan).Append("_To_").Append(dstSan).Append('(');
        if (_srcIsStruct2)
            sb.Append("in ");
        sb.Append(srcType).AppendLine(" source)");
        sb.AppendLine("\t\t{");

        EmitMappingBody(sb, source, dest, enums, names, "\t\t\t");

        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }

    private static void EmitMappingBody(StringBuilder sb, INamedTypeSymbol source, INamedTypeSymbol dest, List<INamedTypeSymbol> enums, NameCache names,
        string indent)
    {
        if (source.TypeKind != TypeKind.Struct)
        {
            sb.Append(indent).AppendLine("if (source is null)");
            sb.Append(indent).AppendLine("\treturn null!;");
            sb.AppendLine();
        }

        // Handle direct collection-to-collection adaptations
        if (Types.IsAnyDictionary(source, out ITypeSymbol? srcKey, out ITypeSymbol? srcValue) &&
            Types.IsAnyDictionary(dest, out ITypeSymbol? dstKey, out ITypeSymbol? dstValue))
        {
            DictionaryEmitter.EmitDictionaryMappingInstructions(sb, source, dest, srcKey!, srcValue!, dstKey!, dstValue!, names, indent);
            return;
        }

        if (Types.IsAnyList(source, out ITypeSymbol? srcElem) && Types.IsAnyList(dest, out ITypeSymbol? dstElem))
        {
            ListEmitter.EmitListMappingInstructions(sb, source, dest, srcElem!, dstElem!, names, indent);
            return;
        }

        // Handle IEnumerable<T> to List<T> adaptations
        if (Types.IsIEnumerable(source, out ITypeSymbol? srcElement) && Types.IsAnyList(dest, out ITypeSymbol? dstElement))
        {
            ListEmitter.EmitListMappingInstructions(sb, source, dest, srcElement!, dstElement!, names, indent);
            return;
        }

        // Delegate to simple object mapper for regular object-to-object mappings
        SimpleObjectEmitter.EmitMappingBodyInstructions(sb, source, dest, enums, names, indent);
    }

    // All mapping logic has been moved to dedicated emitters:
    // - SimpleObjectEmitter.cs: Simple object-to-object mapping
    // - ListEmitter.cs: List and enumerable mapping
    // - DictionaryEmitter.cs: Dictionary mapping
}