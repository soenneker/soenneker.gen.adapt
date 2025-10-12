using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Soenneker.Gen.Adapt;

internal sealed class NameCache
{
    private readonly Dictionary<ISymbol, string> _fq;
    private readonly Dictionary<ISymbol, string> _san;

    public NameCache(int capacity)
    {
        // Pre-size via ctor; works on older TFMs.
        _fq = new Dictionary<ISymbol, string>(capacity, SymbolEqualityComparer.Default);
        _san = new Dictionary<ISymbol, string>(capacity, SymbolEqualityComparer.Default);
    }

    public void Prime(ISymbol symbol)
    {
        if (!_fq.ContainsKey(symbol))
            _fq[symbol] = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (!_san.ContainsKey(symbol))
            _san[symbol] = Sanitize(_fq[symbol]);
    }

    public string FullyQualified(ISymbol s)
    {
        if (_fq.TryGetValue(s, out string? v)) return v;
        v = s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        _fq[s] = v;
        if (!_san.ContainsKey(s)) _san[s] = Sanitize(v);
        return v;
    }

    public string Sanitized(ISymbol s)
    {
        if (_san.TryGetValue(s, out string? v)) return v;
        string fq = FullyQualified(s);
        v = Sanitize(fq);
        _san[s] = v;
        return v;
    }

    private static string Sanitize(string s)
    {
        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            // Allow only [A-Za-z0-9]; everything else becomes '_'
            if ((ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= '0' && ch <= '9'))
            {
                sb.Append(ch);
            }
            else
            {
                sb.Append('_');
            }
        }
        return sb.ToString();
    }
}