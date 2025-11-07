using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Soenneker.Gen.Adapt;

internal static class TypeResolver
{
    /// <summary>
    /// Traces an identifier back to its source by looking at variable declarations and finding Adapt calls.
    /// For example, if we see "doc.Adapt&lt;Foo&gt;()" and doc is declared as "var doc = entity.Adapt&lt;Bar&gt;()",
    /// this will return Bar as the type.
    /// </summary>
    public static INamedTypeSymbol? TraceIdentifierToAdaptCall(IdentifierNameSyntax identifier, SemanticModel model)
    {
        // Try to get the symbol for this identifier
        SymbolInfo symbolInfo = model.GetSymbolInfo(identifier);
        var localSymbol = symbolInfo.Symbol as ILocalSymbol;
        
        if (localSymbol != null)
        {
            // Find the variable declarator for this local symbol
            SyntaxReference? syntaxReference = localSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference != null)
            {
                SyntaxNode declaratorSyntax = syntaxReference.GetSyntax();
                
                // Check if it's a VariableDeclaratorSyntax with an initializer
                if (declaratorSyntax is VariableDeclaratorSyntax { Initializer.Value: not null } declarator)
                {
                    // Check if the initializer is an Adapt call
                    if (declarator.Initializer.Value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax
                        {
                            Identifier.Text: "Adapt", TypeArgumentList.Arguments.Count: > 0
                        } genericName } })
                    {
                        // Get the generic type argument - this is what the variable's type will be
                        TypeSyntax destTypeSyntax = genericName.TypeArgumentList.Arguments[0];
                        ITypeSymbol? destTypeSymbol = model.GetTypeInfo(destTypeSyntax).Type;
                        return destTypeSymbol as INamedTypeSymbol;
                    }
                }
            }
        }
        else
        {
            // Symbol couldn't be resolved (e.g. because type depends on our generator)
            // Fall back to syntax-only search in the containing method/block
            MethodDeclarationSyntax? containingMethod = identifier.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod != null)
            {
                string identifierName = identifier.Identifier.Text;
                
                // Find variable declarations in the method before this usage
                IEnumerable<VariableDeclaratorSyntax> declarators = containingMethod.DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>()
                    .Where(v => v.Identifier.Text == identifierName);
                
                foreach (VariableDeclaratorSyntax? declarator in declarators)
                {
                    // Check if this declarator comes before our identifier in the source
                    if (declarator.SpanStart < identifier.SpanStart && declarator.Initializer?.Value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax
                        {
                            Identifier.Text: "Adapt", TypeArgumentList.Arguments.Count: > 0
                        } genericName } })
                        // Check if the initializer is an Adapt call
                    {
                        // Get the generic type argument - this is what the variable's type will be
                        TypeSyntax destTypeSyntax = genericName.TypeArgumentList.Arguments[0];
                        ITypeSymbol? destTypeSymbol = model.GetTypeInfo(destTypeSyntax).Type;
                        return destTypeSymbol as INamedTypeSymbol;
                    }
                }
            }
        }

        return null;
    }

    public static INamedTypeSymbol? FindTypeByName(Compilation compilation, string typeName)
    {
        INamedTypeSymbol? direct = FindDirectType(compilation, typeName);
        if (direct is not null)
            return direct;

        if (typeName.Contains('.'))
            return ResolvePropertyPathType(compilation, typeName);

        return null;
    }

    private static INamedTypeSymbol? FindDirectType(Compilation compilation, string typeName)
    {
        // Handle generic types like "List<int>" or "Dictionary<string, int>"
        if (typeName.Contains('<'))
        {
            int anglePos = typeName.IndexOf('<');
            string baseTypeName = typeName.Substring(0, anglePos);

            INamedTypeSymbol? baseType = FindDirectType(compilation, baseTypeName);
            if (baseType == null || !baseType.IsGenericType)
                return null;

            string typeArgsStr = typeName.Substring(anglePos + 1, typeName.Length - anglePos - 2);
            List<string> typeArgNames = ParseGenericTypeArguments(typeArgsStr);

            var typeArgs = new List<ITypeSymbol>(typeArgNames.Count);
            foreach (string typeArgName in typeArgNames)
            {
                INamedTypeSymbol? typeArg = FindTypeByName(compilation, typeArgName.Trim());
                if (typeArg == null)
                    return null;
                typeArgs.Add(typeArg);
            }

            if (typeArgs.Count != baseType.TypeParameters.Length)
                return null;

            return baseType.Construct(typeArgs.ToArray());
        }

        INamedTypeSymbol? type = compilation.GetTypeByMetadataName(typeName);
        if (type != null)
            return type;

        if (TryGetFrameworkType(compilation, typeName, out INamedTypeSymbol? frameworkType))
            return frameworkType;

        foreach (INamedTypeSymbol t in GetAllTypes(compilation))
        {
            if (t.Name == typeName)
                return t;

            string display = t.ToDisplayString();
            if (display == typeName || t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == typeName)
                return t;
        }

        return null;
    }

    private static INamedTypeSymbol? ResolvePropertyPathType(Compilation compilation, string typeName)
    {
        string[] segments = typeName.Split(new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            return null;

        INamedTypeSymbol? currentType = FindDirectType(compilation, segments[0]);
        if (currentType is null)
            return null;

        ITypeSymbol current = currentType;

        for (int i = 1; i < segments.Length; i++)
        {
            string memberName = segments[i];
            ITypeSymbol? next = null;

            if (current is INamedTypeSymbol named)
            {
                foreach (ISymbol member in named.GetMembers(memberName))
                {
                    if (member is IPropertySymbol property)
                    {
                        next = property.Type;
                        break;
                    }
                    if (member is IFieldSymbol field)
                    {
                        next = field.Type;
                        break;
                    }
                }
            }

            if (next is null)
                return null;

            current = next;
        }

        return current as INamedTypeSymbol;
    }
    
    private static bool TryGetFrameworkType(Compilation compilation, string typeName, out INamedTypeSymbol? result)
    {
        result = null;
        
        // Common framework types that might be referenced by simple name
        var commonTypes = new[]
        {
            ("List", "System.Collections.Generic.List`1"),
            ("Dictionary", "System.Collections.Generic.Dictionary`2"),
            ("HashSet", "System.Collections.Generic.HashSet`1"),
            ("IEnumerable", "System.Collections.Generic.IEnumerable`1"),
            ("IList", "System.Collections.Generic.IList`1"),
            ("ICollection", "System.Collections.Generic.ICollection`1"),
            ("IDictionary", "System.Collections.Generic.IDictionary`2"),
            ("IReadOnlyList", "System.Collections.Generic.IReadOnlyList`1"),
            ("IReadOnlyCollection", "System.Collections.Generic.IReadOnlyCollection`1"),
            ("IReadOnlyDictionary", "System.Collections.Generic.IReadOnlyDictionary`2"),
            ("ISet", "System.Collections.Generic.ISet`1"),
        };
        
        foreach ((string simpleName, string fullName) in commonTypes)
        {
            if (typeName == simpleName)
            {
                result = compilation.GetTypeByMetadataName(fullName);
                if (result != null)
                    return true;
            }
        }
        
        return false;
    }
    
    private static List<string> ParseGenericTypeArguments(string typeArgsStr)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;
        
        for (var i = 0; i < typeArgsStr.Length; i++)
        {
            char c = typeArgsStr[i];
            if (c == '<')
                depth++;
            else if (c == '>')
                depth--;
            else if (c == ',' && depth == 0)
            {
                result.Add(typeArgsStr.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        
        // Add the last argument
        if (start < typeArgsStr.Length)
        {
            result.Add(typeArgsStr.Substring(start).Trim());
        }
        
        return result;
    }
    
    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
        foreach (INamedTypeSymbol type in ns.GetTypeMembers())
        {
            yield return type;
            foreach (INamedTypeSymbol nested in GetNestedTypes(type))
                yield return nested;
        }
        
        foreach (INamespaceSymbol childNs in ns.GetNamespaceMembers())
        {
            foreach (INamedTypeSymbol type in GetAllTypes(childNs))
                yield return type;
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
    {
        if (compilation.Assembly is { } assembly)
        {
            foreach (INamedTypeSymbol type in GetAllTypes(assembly.GlobalNamespace))
                yield return type;
        }

        foreach (MetadataReference reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol referencedAssembly)
            {
                foreach (INamedTypeSymbol type in GetAllTypes(referencedAssembly.GlobalNamespace))
                    yield return type;
            }
        }
    }
    
    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (INamedTypeSymbol deepNested in GetNestedTypes(nested))
                yield return deepNested;
        }
    }

    /// <summary>
    /// Extracts Adapt calls from Razor file content using regex patterns
    /// </summary>
    public static ImmutableArray<string> ExtractAdaptCallsFromRazor(string content)
    {
        var result = new List<string>();
        
        // First, remove all comment lines to avoid false matches
        string contentWithoutComments = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);
        
        // Find all .Adapt<>() calls - match the whole pattern
        // This handles: variable.Adapt<>, new Type().Adapt<>, method().Adapt<>, obj.Property.Adapt<>
        var adaptCalls = new System.Collections.Generic.List<(string expression, string destType)>();
        
        // Use a more comprehensive regex to find all Adapt calls
        // This handles: variable.Adapt<>, new Type().Adapt<>, method().Adapt<>, obj.Property.Adapt<>
        var adaptCallRegex = new Regex(@"((?:new\s+[a-zA-Z_][\w<>,\s]*?\([^)]*\))|(?:[a-zA-Z_][\w\.]*(?:\([^)]*\))?))\s*\.\s*Adapt\s*<([^>]+)>", RegexOptions.Singleline);
        MatchCollection matches = adaptCallRegex.Matches(contentWithoutComments);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string expression = match.Groups[1].Value.Trim();
                string destType = match.Groups[2].Value.Trim();
                
                if (!string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(expression))
                {
                    adaptCalls.Add((expression, destType));
                }
            }
        }
        
        // Now match adapt calls to declarations
        foreach ((string expression, string destType) in adaptCalls)
        {
            if (IsKeyword(destType))
                continue;
                
            // Try to extract source type from expression
            string sourceType = ExtractSourceTypeFromExpression(expression);
            if (!string.IsNullOrEmpty(sourceType))
            {
                result.Add($"{sourceType}|{destType}");
            }
        }
        
        return [..result];
    }

    private static bool IsKeyword(string typeName)
    {
        var keywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };
        
        return keywords.Contains(typeName.ToLower());
    }

    private static string ExtractSourceTypeFromExpression(string expression)
    {
        // Simple heuristic to extract type from expression
        // This is a basic implementation - could be enhanced for more complex cases
        
        if (expression.StartsWith("new "))
        {
            // Extract type from "new TypeName(...)"
            int parenPos = expression.IndexOf('(');
            if (parenPos > 0)
            {
                string typePart = expression.Substring(4, parenPos - 4).Trim();
                return typePart;
            }
        }
        else if (expression.Contains("."))
        {
            // For property access, try to infer from context
            // This is simplified - in reality would need more sophisticated analysis
            return "object"; // Fallback
        }
        
        return expression;
    }
}
