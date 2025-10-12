using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Soenneker.Gen.Adapt;

internal static class Emitter
{
    /// <summary>
    /// Generates mapping code: enum parsers, per-source mappers, and collection adapters.
    /// </summary>
    public static void Generate(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> classDecls,
        ImmutableArray<INamedTypeSymbol> structDecls,
        ImmutableArray<INamedTypeSymbol> interfaceDecls,
        ImmutableArray<INamedTypeSymbol> enumDecls)
    {
        // Filter to only user-defined symbols in this compilation's assembly and public
        IAssemblySymbol userAsm = compilation.Assembly;

        var userTypes = new List<INamedTypeSymbol>(classDecls.Length + structDecls.Length + interfaceDecls.Length);
        AddUserTypes(userAsm, classDecls, userTypes);
        AddUserTypes(userAsm, structDecls, userTypes);
        AddUserTypes(userAsm, interfaceDecls, userTypes);

        var enums = new List<INamedTypeSymbol>(enumDecls.Length);
        for (int i = 0; i < enumDecls.Length; i++)
        {
            INamedTypeSymbol e = enumDecls[i];
            if (IsUserType(userAsm, e) && IsPublic(e))
                enums.Add(e);
        }

        // Name cache avoids repeated ToDisplayString & sanitization churn
        var nameCache = new NameCache(capacity: userTypes.Count + enums.Count);
        for (int i = 0; i < userTypes.Count; i++) nameCache.Prime(userTypes[i]);
        for (int i = 0; i < enums.Count; i++) nameCache.Prime(enums[i]);

        // Enum parsers
        {
            var sb = new StringBuilder(2048);
            EnumParsers.Emit(sb, enums, nameCache);
            Add(context, "Adapt.EnumParsers.g.cs", sb);
        }

        // Build candidate mapping graph (src -> [dst...]) using pre-indexed props
        Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map = BuildMappingGraph(userTypes, enums);

        // Per-source mapper files
        foreach (KeyValuePair<INamedTypeSymbol, List<INamedTypeSymbol>> kv in map)
        {
            INamedTypeSymbol? source = kv.Key;
            List<INamedTypeSymbol>? destinations = kv.Value;
            if (destinations.Count == 0)
                continue;

            var sb = new StringBuilder(16_384);
            MapperFile.EmitSourceMapperAndDispatcher(sb, source, destinations, enums, nameCache);

            string sanitized = nameCache.Sanitized(source);
            if (sanitized.StartsWith("global__"))
                sanitized = sanitized.Substring("global__".Length);

            var fileName = $"Adapt.{sanitized}.g.cs";

            Add(context, fileName, sb);
        }

        // Collections
        {
            var sb = new StringBuilder(2048);
            Collections.Emit(sb);
            Add(context, "Adapt.Collections.g.cs", sb);
        }
    }


    private static void AddUserTypes(
        IAssemblySymbol userAsm,
        ImmutableArray<INamedTypeSymbol> source,
        List<INamedTypeSymbol> dst)
    {
        for (int i = 0; i < source.Length; i++)
        {
            INamedTypeSymbol t = source[i];
            if (IsUserType(userAsm, t) && IsPublic(t))
                dst.Add(t);
        }
    }

    private static bool IsUserType(IAssemblySymbol asm, INamedTypeSymbol t) =>
        SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, asm);

    private static bool IsPublic(INamedTypeSymbol t) =>
        t.DeclaredAccessibility == Accessibility.Public;

    /// <summary>
    /// Builds mapping graph with pre-indexed properties per type.
    /// </summary>
    private static Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> BuildMappingGraph(
        List<INamedTypeSymbol> types,
        List<INamedTypeSymbol> enums)
    {
        var map = new Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>>(types.Count, SymbolEqualityComparer.Default);

        // Pre-index properties once per type
        var propIndex = new Dictionary<INamedTypeSymbol, TypeProps>(types.Count, SymbolEqualityComparer.Default);
        for (int i = 0; i < types.Count; i++)
            propIndex[types[i]] = TypeProps.Build(types[i]);

        for (int i = 0; i < types.Count; i++)
        {
            INamedTypeSymbol? src = types[i];
            TypeProps? srcProps = propIndex[src];

            for (int j = 0; j < types.Count; j++)
            {
                INamedTypeSymbol? dst = types[j];
                if (SymbolEqualityComparer.Default.Equals(src, dst))
                    continue;

                if (!HasParameterlessCtorLocal(dst))
                    continue;

                TypeProps? dstProps = propIndex[dst];
                if (HasAnyMappableProperty(srcProps, dstProps, enums))
                {
                    if (!map.TryGetValue(src, out List<INamedTypeSymbol>? list))
                    {
                        list = new List<INamedTypeSymbol>(8);
                        map[src] = list;
                    }
                    list.Add(dst);
                }
            }
        }

        return map;
    }

    private static bool HasParameterlessCtorLocal(INamedTypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Struct)
            return true;

        if (type.TypeKind == TypeKind.Interface)
            return false;

        foreach (IMethodSymbol? ctor in type.InstanceConstructors)
            if (ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public)
                return true;

        return false;
    }

    /// <summary>
    /// Checks mapping feasibility using same rules as emission.
    /// </summary>
    private static bool HasAnyMappableProperty(TypeProps src, TypeProps dst, List<INamedTypeSymbol> enums)
    {
        for (int i = 0; i < dst.Settable.Count; i++)
        {
            Prop d = dst.Settable[i];
            if (!src.TryGet(d.Name, out Prop s))
                continue;

            if (Types.IsAnyList(s.Type, out _) && Types.IsAnyList(d.Type, out _))
                return true;

            if (Types.IsAnyDictionary(s.Type, out ITypeSymbol? sKey, out _) && 
                Types.IsAnyDictionary(d.Type, out ITypeSymbol? dKey, out _) &&
                SymbolEqualityComparer.Default.Equals(sKey, dKey))
                return true;

            if (Assignment.CanAssign(s.Type, d.Type, enums))
                return true;
        }
        return false;
    }

    private static void Add(SourceProductionContext ctx, string fileName, StringBuilder sb)
        => ctx.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));
}
