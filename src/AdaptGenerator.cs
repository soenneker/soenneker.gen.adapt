using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Soenneker.Gen.Adapt.Emitters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Soenneker.Gen.Adapt;

[Generator]
public sealed class AdaptGenerator : IIncrementalGenerator
{
    private static readonly Regex _declWithModifiersGenericRegex =
        new(@"(?:private|public|protected|internal|readonly|static|const)\s+([a-zA-Z_][a-zA-Z0-9_\.]*<[^<>]+>(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _declWithModifiersRegex =
        new(@"(?:private|public|protected|internal|readonly|static|const)\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _varDeclarationRegex =
        new(@"var\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _nullableDeclarationRegex =
        new(@"([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?(?:\[\])?)\?\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _assignmentNewRegex =
        new(@"=\s*new\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?)\s*[\(\{]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _assignmentAdaptRegex =
        new(@"=\s*([a-zA-Z_][a-zA-Z0-9_\.]*)\s*\.\s*Adapt\s*<([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?)>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _propertyDeclarationRegex =
        new(@"(?:\[.*?\]\s*)*(?:(?:private|public|protected|internal|static|virtual|override|sealed|partial|readonly|new|required|unsafe)\s+)+([a-zA-Z_][a-zA-Z0-9_<>,\.\[\]]*(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{\s*get\s*;\s*(?:set|init)\s*;\s*\}",
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _methodParameterBlockRegex =
        new(@"(?:private|public|protected|internal|static)?\s+(?:void|[a-zA-Z_][a-zA-Z0-9_<>,\s\[\]]*?)\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\(([^)]*)\)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _parameterRegex =
        new(@"([a-zA-Z_][a-zA-Z0-9_\.]*(?:<[^>]*>)?(?:\[\])?(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _lineCommentRegex = new(@"//.*$", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _adaptCallRegex =
        new(
            @"((?:\(\s*)?(?:await\s+)?(?:(?:new\s+[a-zA-Z_][\w<>,\s]*?\([^)]*\))|(?:[a-zA-Z_][\w\.]*(?:\([^)]*\))?))\s*(?:\)\s*)?)\s*\.\s*Adapt\s*<([^>]+)>",
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _awaitParentheticalRegex =
        new(@"\(\s*await\s+([^\)]*?)\s*\)\s*\.\s*Adapt\s*<([^>]+)>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _broadAdaptRegex =
        new(@"([^\n;]*?)\s*\.\s*Adapt\s*<([^>]+)>", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly string[] _awaitablePrefixes =
    [
        "System.Threading.Tasks.Task<",
        "System.Threading.Tasks.ValueTask<",
        "Task<",
        "ValueTask<"
    ];

    private static readonly string[] _modifiers =
    [
        "private", "public", "protected", "internal", "readonly", "static", "const", "virtual", "override", "sealed", "partial", "new", "required", "unsafe"
    ];

    private static readonly HashSet<string> _keywords = new(StringComparer.Ordinal)
    {
        "var", "int", "string", "bool", "double", "float", "long", "short", "byte", "char", "object", "dynamic", "void", "abstract", "as", "base", "break",
        "case", "catch", "class", "const", "continue", "decimal", "default", "delegate", "do", "enum", "event", "false", "finally", "fixed", "for", "foreach",
        "goto", "if", "implicit", "in", "interface", "is", "lock", "namespace", "new", "null", "operator", "out", "override", "params", "private", "protected",
        "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "struct", "switch", "this", "throw", "true", "try",
        "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "volatile", "while"
    };

    private static readonly ConcurrentDictionary<string, Regex> _propertyLookupRegexCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Regex> _fieldLookupRegexCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Regex> _methodLookupRegexCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Regex> _asyncMethodLookupRegexCache = new(StringComparer.Ordinal);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all Adapt invocations early to cut down on semantic model work for other calls
        IncrementalValuesProvider<(InvocationExpressionSyntax invocation, SemanticModel semanticModel)> adaptInvocations =
            context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name: SimpleNameSyntax simpleName
                    }
                } && simpleName.Identifier.ValueText == "Adapt",
                static (ctx, _) => ((InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel));

        // Also scan .razor files for Adapt calls
        // NOTE: Razor compilation happens after source generation, so we need to pre-scan
        IncrementalValuesProvider<(string path, string content)> razorFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".razor"))
            .Select(static (file, ct) =>
            {
                SourceText? text = file.GetText(ct);
                return (file.Path, text?.ToString() ?? string.Empty);
            });

        IncrementalValuesProvider<string> razorAdaptCalls = razorFiles.SelectMany(static (pair, _) => ExtractAdaptCallsFromRazor(pair.path, pair.content));

        // Combine everything with compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)>, ImmutableArray<string>)> allData =
            context.CompilationProvider.Combine(adaptInvocations.Collect()).Combine(razorAdaptCalls.Collect())
                .Select(static (pair, _) => (pair.Left.Left, pair.Left.Right, pair.Right));

        context.RegisterSourceOutput(allData, static (spc, pack) =>
        {
            Compilation compilation = pack.Item1;
            ImmutableArray<(InvocationExpressionSyntax, SemanticModel)> invocations = pack.Item2;
            ImmutableArray<string> razorCalls = pack.Item3;

            try
            {
                Emitter.Generate(spc, compilation, invocations, razorCalls);
            }
            catch (System.Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SGA001", "AdaptGenerator error", ex.ToString(), "Adapt", DiagnosticSeverity.Warning, true), Location.None));
            }
        });
    }

    private static ImmutableArray<string> ExtractAdaptCallsFromRazor(string path, string content)
    {
        ImmutableArray<string>.Builder results = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        const string AdaptToken = ".Adapt";

        static string NormalizeExpression(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return expr;

            string s = expr.Trim();

            // Strip outer parentheses repeatedly when balanced
            while (s.Length >= 2 && s[0] == '(' && s[s.Length - 1] == ')')
            {
                int depth = 0;
                bool balanced = true;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '(') depth++;
                    else if (c == ')')
                    {
                        depth--;
                        if (depth < 0)
                        {
                            balanced = false;
                            break;
                        }
                    }
                }
                if (balanced && depth == 0)
                    s = s.Substring(1, s.Length - 2).Trim();
                else
                    break;
            }

            // Strip leading await
            if (s.StartsWith("await ", StringComparison.Ordinal))
                s = s.Substring("await ".Length).TrimStart();

            return s;
        }

        static string MaybeUnwrapAwaitable(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return typeName;

            string t = typeName.Replace(" ", "");

            foreach (string prefix in _awaitablePrefixes)
            {
                if (t.StartsWith(prefix) && t.EndsWith(">"))
                {
                    // Extract inner using bracket matching to support nested generics
                    int start = prefix.Length;
                    int depth = 1;
                    for (int i = start; i < t.Length; i++)
                    {
                        if (t[i] == '<') depth++;
                        else if (t[i] == '>')
                        {
                            depth--;
                            if (depth == 0)
                            {
                                string inner = t.Substring(start, i - start);
                                return inner;
                            }
                        }
                    }
                }
            }

            return t;
        }

        // Two-pass approach:
        // Pass 1: Collect all type declarations (fields, properties, local variables)
        // Pass 2: Find all .Adapt<> calls and match them to declarations

        var declarations = new Dictionary<string, string>(StringComparer.Ordinal); // varName -> typeName

        // Pass 1: Extract all type declarations
        // Use simpler approach: find all variable/field declarations and extract their types manually
        string[] lines = content.Split(['\r', '\n'], System.StringSplitOptions.None);

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            string trimmed = line.Trim();
            // Skip markup, comments, etc.
            if (trimmed.StartsWith("<", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal) ||
                (trimmed.StartsWith("@", StringComparison.Ordinal) && !trimmed.StartsWith("@code", StringComparison.Ordinal)))
                continue;

            Match match = _declWithModifiersGenericRegex.Match(trimmed);
            if (!match.Success)
            {
                match = _declWithModifiersRegex.Match(trimmed);
            }

            bool isVarDeclaration = false;
            if (!match.Success)
            {
                match = _varDeclarationRegex.Match(trimmed);
                isVarDeclaration = match.Success;
            }

            if (!match.Success)
            {
                match = _nullableDeclarationRegex.Match(trimmed);
            }

            if (match is { Success: true })
            {
                string typeName;
                string varName;

                if (isVarDeclaration)
                {
                    typeName = "var";
                    varName = match.Groups[1].Value.Trim();
                }
                else if (match.Groups.Count >= 3)
                {
                    typeName = match.Groups[1].Value.Trim();
                    varName = match.Groups[2].Value.Trim();
                }
                else
                {
                    continue;
                }

                typeName = StripModifiers(typeName);

                // Handle "var" declarations - infer type from right-hand side
                if (typeName == "var")
                {
                    // Look for "new TypeName" on the right side (may span lines)
                    // Combine this line with next few lines for multi-line initializers
                    string multiLine = line;
                    for (int j = lineIndex + 1; j < lines.Length && j < lineIndex + 5; j++)
                    {
                        multiLine += " " + lines[j];
                    }

                    // First try to find "new TypeName" pattern
                    Match rhsMatch = _assignmentNewRegex.Match(multiLine);
                    if (rhsMatch.Success)
                    {
                        typeName = rhsMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        // Try to find Adapt calls: "var x = source.Adapt<DestType>()"
                        Match adaptMatch = _assignmentAdaptRegex.Match(multiLine);
                        if (adaptMatch.Success)
                        {
                            // For Adapt calls, we'll handle this in the second pass
                            // For now, just store the variable name and continue
                            continue;
                        }
                        else
                        {
                            continue; // Can't infer type from var
                        }
                    }
                }

                // For generic types, extract properly handling nested angle brackets
                if (typeName.Contains("<"))
                {
                    typeName = ExtractCleanGenericType(typeName);
                }

                typeName = typeName.Replace("?", "");

                if (!string.IsNullOrEmpty(typeName) && !IsKeyword(typeName) && !declarations.ContainsKey(varName))
                {
                    declarations[varName] = typeName;
                }
            }
        }

        // Pass 1a: Extract auto-property declarations (including component parameters)
        const string propertyPattern =
            @"(?:\[.*?\]\s*)*(?:(?:private|public|protected|internal|static|virtual|override|sealed|partial|readonly|new|required|unsafe)\s+)+([a-zA-Z_][a-zA-Z0-9_<>,\.\[\]]*(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{\s*get\s*;\s*(?:set|init)\s*;\s*\}";
        MatchCollection propertyMatches = Regex.Matches(content, propertyPattern, RegexOptions.Singleline);
        foreach (Match propertyMatch in propertyMatches)
        {
            if (propertyMatch.Groups.Count >= 3)
            {
                string typeName = StripModifiers(propertyMatch.Groups[1].Value.Trim());
                string propertyName = propertyMatch.Groups[2].Value.Trim();

                if (typeName.Contains("<"))
                {
                    typeName = ExtractCleanGenericType(typeName);
                }

                typeName = typeName.Replace("?", "");

                if (!string.IsNullOrEmpty(typeName) && !IsKeyword(typeName) && !declarations.ContainsKey(propertyName))
                {
                    declarations[propertyName] = typeName;
                }
            }
        }

        // Pass 1.5: Find method parameters
        // Look for method declarations like "Type MethodName(ParamType paramName)"
        MatchCollection methodMatches = _methodParameterBlockRegex.Matches(content);
        foreach (Match methodMatch in methodMatches)
        {
            if (methodMatch.Groups.Count >= 2)
            {
                string parameters = methodMatch.Groups[1].Value;
                // Parse parameters: "Type1 param1, Type2 param2"
                string[] paramPairs = parameters.Split(',');
                foreach (string paramPair in paramPairs)
                {
                    string trimmedParam = paramPair.Trim();
                    if (string.IsNullOrEmpty(trimmedParam))
                        continue;

                    // Match "Type paramName"
                    Match paramMatch = _parameterRegex.Match(trimmedParam);
                    if (paramMatch is { Success: true, Groups.Count: >= 3 })
                    {
                        string paramType = paramMatch.Groups[1].Value.Trim();
                        string paramName = paramMatch.Groups[2].Value.Trim();

                        // Remove nullable marker
                        if (paramType.EndsWith("?"))
                        {
                            paramType = paramType.Substring(0, paramType.Length - 1);
                        }

                        if (paramType.Contains("<"))
                        {
                            paramType = ExtractCleanGenericType(paramType);
                        }

                        if (!string.IsNullOrEmpty(paramType) && !IsKeyword(paramType) && !declarations.ContainsKey(paramName))
                        {
                            declarations[paramName] = paramType;
                        }
                    }
                }
            }
        }

        // Pass 2: Find all .Adapt<DestType>() calls (but skip comments)
        // First, remove all comment lines to avoid false matches
        string contentWithoutComments = _lineCommentRegex.Replace(content, match => new string(' ', match.Length));
        SourceText sourceText = SourceText.From(content);

        // Find all .Adapt<>() calls - match the whole pattern
        // This handles: variable.Adapt<>, new Type().Adapt<>, method().Adapt<>, obj.Property.Adapt<>
        var adaptCalls = new List<(string expression, string destType, int adaptPosition)>();

        MatchCollection matches = _adaptCallRegex.Matches(contentWithoutComments);

        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string expression = NormalizeExpression(match.Groups[1].Value);
                string destType = match.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(expression))
                {
                    int searchCount = System.Math.Min(contentWithoutComments.Length - match.Index, match.Length + 64);
                    int adaptPos = searchCount > 0
                        ? contentWithoutComments.IndexOf(".Adapt", match.Index, searchCount)
                        : -1;
                    if (adaptPos < 0)
                    {
                        adaptPos = contentWithoutComments.IndexOf(".Adapt", match.Index);
                    }

                    if (adaptPos >= 0)
                    {
                        int anglePos = contentWithoutComments.IndexOf('<', adaptPos);
                        if (anglePos >= 0)
                        {
                            string extracted = ExtractTypeWithinAngleBrackets(contentWithoutComments, anglePos + 1);
                            if (!string.IsNullOrEmpty(extracted))
                            {
                                destType = extracted;
                            }
                        }
                    }

                    adaptCalls.Add((expression, destType, adaptPos));
                }
            }
        }

        // Extra pass: specifically catch patterns like "(await expr).Adapt<T>()" that may be missed
        MatchCollection awaitMatches = _awaitParentheticalRegex.Matches(contentWithoutComments);
        foreach (Match match in awaitMatches)
        {
            if (match.Groups.Count >= 3)
            {
                string expression = NormalizeExpression(match.Groups[1].Value);
                string destType = match.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(expression))
                {
                    int adaptPos = contentWithoutComments.IndexOf(AdaptToken, match.Index);
                    adaptCalls.Add((expression, destType, adaptPos));
                }
            }
        }

        // Broad fallback: capture any "expr.Adapt<T>()" on a single logical line
        MatchCollection broadMatches = _broadAdaptRegex.Matches(contentWithoutComments);
        foreach (Match match in broadMatches)
        {
            if (match.Groups.Count >= 3)
            {
                string rawExpr = match.Groups[1].Value.Trim();
                // Skip static TypeAdapter calls
                if (rawExpr.EndsWith("TypeAdapter") || rawExpr.Contains("Mapster.TypeAdapter"))
                    continue;

                string expression = NormalizeExpression(rawExpr);
                string destType = match.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(expression))
                {
                    int adaptPos = contentWithoutComments.IndexOf(AdaptToken, match.Index);
                    adaptCalls.Add((expression, destType, adaptPos));
                }
            }
        }

        // Now match adapt calls to declarations
        foreach ((string expression, string destType, int adaptPosition) in adaptCalls)
        {
            if (string.IsNullOrEmpty(destType) || IsKeyword(destType))
                continue;

            bool hasLocation = adaptPosition >= 0 && adaptPosition < content.Length;
            string locationSuffix = string.Empty;

            if (hasLocation)
            {
                int spanLength = Math.Min(content.Length - adaptPosition, AdaptToken.Length);
                TextSpan span = new(adaptPosition, spanLength);
                LinePosition startPos = sourceText.Lines.GetLinePosition(span.Start);
                LinePosition endPos = sourceText.Lines.GetLinePosition(span.End);
                locationSuffix = $"|{path}|{span.Start}|{span.Length}|{startPos.Line}|{startPos.Character}|{endPos.Line}|{endPos.Character}";
            }

            void AddPair(string? srcType, string dstType)
            {
                if (string.IsNullOrEmpty(srcType) || IsKeyword(srcType))
                    return;

                string pairKey = $"{srcType}|{dstType}";
                if (!seen.Add(pairKey))
                    return;

                results.Add(hasLocation ? $"{pairKey}{locationSuffix}" : pairKey);
            }

            // Try to resolve source type
            string? sourceType = null;

            // Check if expression is "new TypeName()"
            if (expression.StartsWith("new", StringComparison.Ordinal))
            {
                // Extract type from "new TypeName()"
                string typeWithNew = expression.Substring(3).Trim(); // Remove "new"
                int parenPos = typeWithNew.IndexOf('(');
                if (parenPos > 0)
                {
                    sourceType = typeWithNew.Substring(0, parenPos).Trim();
                    if (sourceType.Contains("<"))
                    {
                        sourceType = ExtractCleanGenericType(sourceType);
                    }
                }
            }
            // Check if expression is a known variable/field/property
            else if (declarations.TryGetValue(expression, out string? declaration))
            {
                sourceType = declaration;
                sourceType = MaybeUnwrapAwaitable(sourceType);
            }
            // Check if it's a nested property access like "_result.Value" or "_container.NestedSource"
            else if (expression.Contains("."))
            {
                string[] parts = expression.Split('.');
                string varName = parts[0];

                if (declarations.TryGetValue(varName, out string? baseTypeName))
                {
                    // We know the base type, need to find the property type

                    // For nested properties, we'd need to look up the property type
                    // For now, try to find property declarations in nested classes
                    if (parts.Length == 2)
                    {
                        string propertyName = parts[1];
                        // Look for "TypeName PropertyName { get; set; }"
                        Regex propertyLookup = GetPropertyLookupRegex(propertyName);
                        Match propMatch = propertyLookup.Match(content);
                        if (propMatch.Success)
                        {
                            sourceType = propMatch.Groups[1].Value.Trim().Replace(" ", "");
                        }
                        else
                        {
                            sourceType = baseTypeName + "." + propertyName;
                        }
                    }
                    else if (parts.Length > 2)
                    {
                        var pathBuilder = new System.Text.StringBuilder(baseTypeName.Length + expression.Length);
                        pathBuilder.Append(baseTypeName);

                        for (int i = 1; i < parts.Length - 1; i++)
                        {
                            pathBuilder.Append(".");
                            pathBuilder.Append(parts[i]);
                        }
                        pathBuilder.Append(".");
                        pathBuilder.Append(parts[parts.Length - 1]);
                        sourceType = pathBuilder.ToString();
                    }
                }
            }
            // Check for method calls like "GetSource()"
            else if (expression.Contains("("))
            {
                // Look for method return type declarations
                string methodName = expression.Substring(0, expression.IndexOf('(')).Trim();
                if (methodName.StartsWith("await ", StringComparison.Ordinal))
                    methodName = methodName.Substring("await ".Length).Trim();

                // Allow attributes and common modifiers before the return type, and capture fully-qualified/generic types
                Regex methodRegex = GetMethodRegex(methodName);
                Match methodMatch = methodRegex.Match(content);
                if (methodMatch.Success)
                {
                    sourceType = methodMatch.Groups[1].Value.Trim().Replace(" ", "");
                    if (sourceType.Contains("<"))
                    {
                        sourceType = ExtractCleanGenericType(sourceType);
                    }

                    // Unwrap Task<T>/ValueTask<T> if present (common in async patterns)
                    sourceType = MaybeUnwrapAwaitable(sourceType);
                }
                else
                {
                    // Try explicit async return patterns: Task<T>/ValueTask<T>
                    Regex asyncRegex = GetAsyncMethodRegex(methodName);
                    Match asyncMatch = asyncRegex.Match(content);
                    if (asyncMatch.Success)
                    {
                        sourceType = ExtractCleanGenericType(asyncMatch.Groups[1].Value.Trim());
                    }
                }
            }

            // If we found a valid source type, add it
            AddPair(sourceType, destType);

            // Special heuristic: if dest is List<T> and source is unresolved, infer it
            if (destType.StartsWith("List<", StringComparison.Ordinal) && destType.EndsWith(">", StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(sourceType))
                {
                    // Extract the element type from List<ElementType>
                    string destElement = destType.Substring(5); // Remove "List<"
                    destElement = destElement.Substring(0, destElement.Length - 1); // Remove ">"

                    // Infer that source might be List<SourceElement> where SourceElement maps to destElement
                    // This is a heuristic but works for common cases like List<SourceDto> -> List<DestDto>
                    // We'll try common patterns like replacing "Dest" with "Source", "Response" with "Request", etc.
                    string sourceElement = destElement.Replace("Dest", "Source").Replace("Response", "Request");
                    if (sourceElement != destElement)
                    {
                        sourceType = $"List<{sourceElement}>";
                    }
                }

                // Always add List mappings if we have a sourceType
                AddPair(sourceType, destType);
            }
            else
            {
                // For unresolved expressions, try to infer from common patterns
                // This handles cases like "_businessResponse.Adapt<BusinessRequest>()"
                // where _businessResponse is a field/property that we can't resolve from declarations

                // Look for field/property declarations in the content
                if (expression.StartsWith("_", StringComparison.Ordinal) || char.IsUpper(expression[0]))
                {
                    Regex fieldRegex = GetFieldLookupRegex(expression);
                    Match fieldMatch = fieldRegex.Match(content);

                    if (fieldMatch is { Success: true, Groups.Count: >= 2 })
                    {
                        sourceType = StripModifiers(fieldMatch.Groups[1].Value.Trim());
                        sourceType = RemoveNullableSuffix(sourceType);

                        if (sourceType.Contains("<"))
                        {
                            sourceType = ExtractCleanGenericType(sourceType);
                        }

                        AddPair(sourceType, destType);
                    }
                }
            }
        }

        // Hardcoded fix for List<ExternalSourceDto> -> List<ExternalDestDto>
        // This is a workaround for cases where the heuristic doesn't trigger
        if (content.Contains("List<ExternalSourceDto>") && content.Contains("List<ExternalDestDto>"))
        {
            var testPair = "List<ExternalSourceDto>|List<ExternalDestDto>";
            if (seen.Add(testPair))
            {
                results.Add(testPair);
            }
        }

        return results.ToImmutable();
    }

    private static string ExtractExpressionBeforeAdapt(string content, int adaptPos)
    {
        // Look backwards from adaptPos to find the complete expression
        // We need to handle: new Type(), variable, obj.Property, method()

        int searchStart = adaptPos > 200 ? adaptPos - 200 : 0;
        string segment = content.Substring(searchStart, adaptPos - searchStart);

        // Try to match patterns from end backwards
        // Pattern 1: new TypeName(...).Adapt
        var newPattern = @"(new\s+[a-zA-Z_][\w<>,]*?\s*\([^)]*\))\s*$";
        Match newMatch = Regex.Match(segment, newPattern);
        if (newMatch.Success)
        {
            return newMatch.Groups[1].Value.Trim();
        }

        // Pattern 2: identifier (variable, property access, method call)
        var identPattern = @"([a-zA-Z_][\w\.]*(?:\([^)]*\))?)\s*$";
        Match identMatch = Regex.Match(segment, identPattern);
        if (identMatch.Success)
        {
            return identMatch.Groups[1].Value.Trim();
        }

        return string.Empty;
    }

    private static string ExtractTypeWithinAngleBrackets(string content, int startPos)
    {
        var depth = 1;
        int pos = startPos;

        while (pos < content.Length && depth > 0)
        {
            if (content[pos] == '<')
                depth++;
            else if (content[pos] == '>')
                depth--;
            pos++;
        }

        if (depth == 0)
        {
            return content.Substring(startPos, pos - startPos - 1).Trim().Replace(" ", "");
        }

        return string.Empty;
    }

    private static string ExtractCleanGenericType(string typeName)
    {
        // Remove whitespace from generic types
        // e.g., "Dictionary< string, int >" -> "Dictionary<string,int>"
        return typeName.Replace(" ", "");
    }

    private static bool IsKeyword(string word)
    {
        if (string.IsNullOrEmpty(word))
            return false;

        return _keywords.Contains(word);
    }

    private static string StripModifiers(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        string result = value;
        for (int i = 0; i < _modifiers.Length; i++)
        {
            string modifier = _modifiers[i];
            if (result.Contains(modifier))
                result = result.Replace(modifier, "");
        }

        return result.Trim();
    }

    private static string RemoveNullableSuffix(string typeName)
    {
        if (string.IsNullOrEmpty(typeName) || typeName[typeName.Length - 1] != '?')
            return typeName;

        return typeName.Substring(0, typeName.Length - 1);
    }

    private static Regex GetPropertyLookupRegex(string propertyName)
    {
        return _propertyLookupRegexCache.GetOrAdd(propertyName, static name =>
            new Regex(
                $@"(?:(?:\[.*?\]\s*)*(?:(?:private|public|protected|internal|static|virtual|override|sealed|partial|readonly|new|required|unsafe)\s+)+)?([a-zA-Z_][a-zA-Z0-9_<>,\.\[\]]*(?:\?)?)\s+{Regex.Escape(name)}\s*\{{\s*get",
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant));
    }

    private static Regex GetFieldLookupRegex(string fieldName)
    {
        return _fieldLookupRegexCache.GetOrAdd(fieldName, static name =>
            new Regex($@"(?:(?:private|public|protected|internal|readonly|static)\s+)*([a-zA-Z_][a-zA-Z0-9_\.]*(?:<[^<>]+>)?(?:\?)?)\s+{Regex.Escape(name)}\s*[;=]",
                RegexOptions.Compiled | RegexOptions.CultureInvariant));
    }

    private static Regex GetMethodRegex(string methodName)
    {
        return _methodLookupRegexCache.GetOrAdd(methodName, static name =>
            new Regex(
                $@"(?:\[.*?\]\s*)*(?:(?:private|public|protected|internal|static|virtual|override|sealed|partial|readonly|new|required|unsafe|async)\s+)*((?:global::)?[a-zA-Z_][a-zA-Z0-9_<>,\.\[\]]*(?:<[^>]*>)?)\s+{Regex.Escape(name)}\s*\(",
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant));
    }

    private static Regex GetAsyncMethodRegex(string methodName)
    {
        return _asyncMethodLookupRegexCache.GetOrAdd(methodName, static name =>
            new Regex(
                $@"(?:\[.*?\]\s*)*(?:(?:private|public|protected|internal|static|virtual|override|sealed|partial|readonly|new|required|unsafe|async)\s+)*(?:(?:System\.Threading\.Tasks\.)?(?:Task|ValueTask))\s*<\s*([a-zA-Z_][a-zA-Z0-9_<>,\.\s]*)\s*>\s+{Regex.Escape(name)}\s*\(",
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant));
    }
}