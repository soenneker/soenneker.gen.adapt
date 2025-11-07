using System.Collections.Generic;

namespace Soenneker.Gen.Adapt;

internal static class NamespaceHelper
{
    private static readonly (string Prefix, string Namespace)[] _prefixes = new (string, string)[]
    {
        ("global::System.Collections.Generic.", "System.Collections.Generic"),
        ("global::System.Collections.Concurrent.", "System.Collections.Concurrent"),
        ("global::System.Collections.Immutable.", "System.Collections.Immutable"),
        ("global::System.Collections.ObjectModel.", "System.Collections.ObjectModel"),
        ("global::System.Runtime.InteropServices.", "System.Runtime.InteropServices"),
    };

    public static string ToShortName(string fq, HashSet<string>? sink)
    {
        for (var i = 0; i < _prefixes.Length; i++)
        {
            (string prefix, string ns) = _prefixes[i];
            if (fq.Contains(prefix))
            {
                sink?.Add(ns);
                fq = fq.Replace(prefix, string.Empty);
            }
        }

        return fq;
    }
}

