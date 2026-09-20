using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Soenneker.Gen.Adapt.Emitters;

internal static class EnumEmitter
{
    public static void Emit(StringBuilder sb, List<INamedTypeSymbol> enums, NameCache names, string targetNamespace)
    {
        sb.AppendLine("using System;");
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine();
        sb.Append("namespace ").AppendLine(targetNamespace);
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static partial class GenAdapt_EnumParsers");
        sb.AppendLine("\t{");

        for (var i = 0; i < enums.Count; i++)
        {
            INamedTypeSymbol? e = enums[i];
            string fq = names.ShortName(e);
            string sanitized = names.Sanitized(e);

            // gather enum member names (fields with constant value)
            List<string> members = Types.GetEnumMemberNames(e);

            sb.AppendLine($"\t\t[GeneratedCode(\"{GeneratorMetadata.Name}\", \"{GeneratorMetadata.Version}\")] ");
            sb.AppendLine("\t\t[ExcludeFromCodeCoverage]");
            sb.Append("\t\tinternal static ").Append(fq).Append(" Parse_").Append(sanitized).AppendLine("(string value)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tswitch (value)");
            sb.AppendLine("\t\t\t{");
            for (var m = 0; m < members.Count; m++)
            {
                string? name = members[m];
                sb.Append("\t\t\t\tcase \"").Append(name).Append("\": return ").Append(fq).Append('.').Append(name).AppendLine(";");
            }
            sb.Append("\t\t\t\tdefault: throw new ArgumentOutOfRangeException(nameof(value), $\"Unknown enum value '{value}' for ")
                .Append(e.Name).AppendLine("\");");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            EmitFormatter(sb, e, fq, sanitized);
        }

        sb.AppendLine("\t}");
        sb.AppendLine("}");
    }

    private static void EmitFormatter(StringBuilder sb, INamedTypeSymbol type, string typeName, string sanitized)
    {
        var members = type.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue)
            .Select(f => (Name: f.Name, Bits: GetBits(f.ConstantValue!)))
            .GroupBy(f => f.Bits).Select(g => g.First()).OrderBy(f => f.Bits).ToList();
        string underlying = Types.Fq(type.EnumUnderlyingType!);
        sb.Append("\t\tinternal static string Format_").Append(sanitized).Append('(').Append(typeName).AppendLine(" value)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\tswitch (value)");
        sb.AppendLine("\t\t\t{");
        foreach (var member in members)
            sb.Append("\t\t\t\tcase ").Append(typeName).Append(".@").Append(member.Name).Append(": return \"").Append(member.Name).AppendLine("\";");
        sb.AppendLine("\t\t\t}");

        if (type.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.FlagsAttribute") && members.Any(m => m.Bits != 0))
        {
            var flags = members.Where(m => m.Bits != 0).Reverse().ToList();
            sb.Append("\t\t\tulong remaining = unchecked((ulong)(").Append(underlying).AppendLine(")value);");
            sb.AppendLine("\t\t\tint length = 0;");
            foreach (var flag in flags)
            {
                string bits = flag.Bits.ToString(CultureInfo.InvariantCulture) + "UL";
                sb.Append("\t\t\tif ((remaining & ").Append(bits).Append(") == ").Append(bits).AppendLine(")");
                sb.AppendLine("\t\t\t{");
                sb.Append("\t\t\t\tremaining &= ~").Append(bits).AppendLine(";");
                sb.Append("\t\t\t\tlength += ").Append(flag.Name.Length + 2).AppendLine(";");
                sb.AppendLine("\t\t\t}");
            }
            sb.AppendLine("\t\t\tif (remaining == 0 && length != 0)");
            sb.AppendLine("\t\t\t{");
            // Allocate only the output string: no enum boxing, metadata lookup,
            // temporary name arrays, intermediate strings or captured closures.
            sb.Append("\t\t\t\treturn string.Create(length - 2, unchecked((ulong)(").Append(underlying).AppendLine(")value), static (span, bits) =>");
            sb.AppendLine("\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\tint end = span.Length;");
            foreach (var flag in flags)
            {
                string bits = flag.Bits.ToString(CultureInfo.InvariantCulture) + "UL";
                sb.Append("\t\t\t\t\tif ((bits & ").Append(bits).Append(") == ").Append(bits).AppendLine(")");
                sb.AppendLine("\t\t\t\t\t{");
                sb.Append("\t\t\t\t\t\tbits &= ~").Append(bits).AppendLine(";");
                sb.Append("\t\t\t\t\t\tend -= ").Append(flag.Name.Length).AppendLine(";");
                sb.Append("\t\t\t\t\t\t\"").Append(flag.Name).AppendLine("\".AsSpan().CopyTo(span.Slice(end));");
                sb.AppendLine("\t\t\t\t\t\tif (end != 0) { span[--end] = ' '; span[--end] = ','; }");
                sb.AppendLine("\t\t\t\t\t}");
            }
            sb.AppendLine("\t\t\t\t});");
            sb.AppendLine("\t\t\t}");
        }
        sb.Append("\t\t\treturn ((").Append(underlying).AppendLine(")value).ToString();");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
    }

    private static ulong GetBits(object value) => value switch
    {
        sbyte v => unchecked((ulong)v),
        short v => unchecked((ulong)v),
        int v => unchecked((ulong)v),
        long v => unchecked((ulong)v),
        _ => Convert.ToUInt64(value, CultureInfo.InvariantCulture)
    };
}
