using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Soenneker.Gen.Adapt;

[Generator]
public sealed class AdaptGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all invocations - we'll filter for Adapt calls in the Emitter
        IncrementalValuesProvider<(InvocationExpressionSyntax, SemanticModel)> adaptInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is InvocationExpressionSyntax,
            static (ctx, _) => ((InvocationExpressionSyntax)ctx.Node, ctx.SemanticModel));

        // Also scan .razor files for Adapt calls
        // NOTE: Razor compilation happens after source generation, so we need to pre-scan
        IncrementalValuesProvider<(string path, string content)> razorFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".razor"))
            .Select(static (file, ct) =>
            {
                SourceText? text = file.GetText(ct);
                return (file.Path, text?.ToString() ?? string.Empty);
            });
            
        IncrementalValuesProvider<string> razorAdaptCalls = razorFiles
            .SelectMany(static (pair, _) => ExtractAdaptCallsFromRazor(pair.content));

        // Combine everything with compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<(InvocationExpressionSyntax, SemanticModel)>, ImmutableArray<string>)> allData = 
            context.CompilationProvider
                .Combine(adaptInvocations.Collect())
                .Combine(razorAdaptCalls.Collect())
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
                    new DiagnosticDescriptor("SGA001", "AdaptGenerator error", ex.ToString(), "Adapt", DiagnosticSeverity.Warning, true),
                    Location.None));
            }
        });
    }

    private static ImmutableArray<string> ExtractAdaptCallsFromRazor(string content)
    {
        var results = ImmutableArray.CreateBuilder<string>();
        var seen = new System.Collections.Generic.HashSet<string>();
        
        // Two-pass approach:
        // Pass 1: Collect all type declarations (fields, properties, local variables)
        // Pass 2: Find all .Adapt<> calls and match them to declarations
        
        var declarations = new System.Collections.Generic.Dictionary<string, string>(); // varName -> typeName
        
        // Pass 1: Extract all type declarations
        // Use simpler approach: find all variable/field declarations and extract their types manually
        var lines = content.Split(new[] { '\r', '\n' }, System.StringSplitOptions.None);
        
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            string trimmed = line.Trim();
            // Skip markup, comments, etc.
            if (trimmed.StartsWith("<") || trimmed.StartsWith("//") || trimmed.StartsWith("@") && !trimmed.StartsWith("@code"))
                continue;
            
            // Look for variable declarations: [modifiers] TypeName varName =
            // Handle both simple and generic types
            var declPattern = @"(?:private|public|protected|internal|readonly|static|const)?\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?(?:\[\])?(?:\?)?)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=";
            var match = Regex.Match(trimmed, declPattern);
            
            if (match.Success && match.Groups.Count >= 3)
            {
                string typeName = match.Groups[1].Value.Trim();
                string varName = match.Groups[2].Value.Trim();
                
                // Remove modifiers if they got captured
                typeName = typeName.Replace("private", "").Replace("public", "").Replace("protected", "")
                    .Replace("internal", "").Replace("readonly", "").Replace("static", "").Replace("const", "").Trim();
                
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
                    
                    var rhsPattern = @"=\s*new\s+([a-zA-Z_][a-zA-Z0-9_\.]*(?:<.+?>)?)\s*[\(\{]";
                    var rhsMatch = Regex.Match(multiLine, rhsPattern);
                    if (rhsMatch.Success)
                    {
                        typeName = rhsMatch.Groups[1].Value.Trim();
                    }
                    else
                    {
                        continue; // Can't infer type from var
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
        
        // Pass 2: Find all .Adapt<DestType>() calls (but skip comments)
        // First, remove all comment lines to avoid false matches
        string contentWithoutComments = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);
        
        // Find all .Adapt<>() calls - match the whole pattern
        // This handles: variable.Adapt<>, new Type().Adapt<>, method().Adapt<>, obj.Property.Adapt<>
        var adaptCallRegex = new Regex(@"((?:new\s+[a-zA-Z_][\w<>,\s]*?\(.*?\))|(?:[a-zA-Z_][\w\.]*?))\s*\.\s*Adapt\s*<", RegexOptions.Singleline);
        var adaptCalls = new System.Collections.Generic.List<(string expression, string destType)>();
        
        int searchPos = 0;
        while (searchPos < contentWithoutComments.Length)
        {
            int adaptPos = contentWithoutComments.IndexOf(".Adapt<", searchPos);
            if (adaptPos == -1)
                break;
            
            // Extract destination type (handle nested generics)
            int typeStart = adaptPos + 7; // ".Adapt<".Length
            string destType = ExtractTypeWithinAngleBrackets(contentWithoutComments, typeStart);
            
            // Extract expression before .Adapt< - look backwards for complete expression
            string expression = ExtractExpressionBeforeAdapt(contentWithoutComments, adaptPos);
            
            if (!string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(expression))
            {
                adaptCalls.Add((expression, destType));
            }
            
            searchPos = adaptPos + 1;
        }
        
        // Now match adapt calls to declarations
        foreach (var (expression, destType) in adaptCalls)
        {
            if (IsKeyword(destType))
                continue;
            
            // Try to resolve source type
            string sourceType = null;
            
            // Check if expression is "new TypeName()"
            if (expression.StartsWith("new"))
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
            else if (declarations.ContainsKey(expression))
            {
                sourceType = declarations[expression];
            }
            // Check if it's a nested property access like "_result.Value" or "_container.NestedSource"
            else if (expression.Contains("."))
            {
                string[] parts = expression.Split('.');
                string varName = parts[0];
                
                if (declarations.ContainsKey(varName))
                {
                    // We know the base type, need to find the property type
                    string baseTypeName = declarations[varName];
                    
                    // For nested properties, we'd need to look up the property type
                    // For now, try to find property declarations in nested classes
                    if (parts.Length == 2)
                    {
                        string propertyName = parts[1];
                        // Look for "TypeName PropertyName { get; set; }"
                        var propPattern = $@"([a-zA-Z_][a-zA-Z0-9_<>,]*?)\s+{Regex.Escape(propertyName)}\s*\{{\s*get";
                        var propMatch = Regex.Match(content, propPattern);
                        if (propMatch.Success)
                        {
                            sourceType = propMatch.Groups[1].Value.Trim().Replace(" ", "");
                        }
                    }
                }
            }
            // Check for method calls like "GetSource()"
            else if (expression.Contains("("))
            {
                // Look for method return type declarations
                string methodName = expression.Substring(0, expression.IndexOf('('));
                var methodPattern = $@"([a-zA-Z_][a-zA-Z0-9_<>,]*?)\s+{Regex.Escape(methodName)}\s*\(";
                var methodMatch = Regex.Match(content, methodPattern);
                if (methodMatch.Success)
                {
                    sourceType = methodMatch.Groups[1].Value.Trim().Replace(" ", "");
                    if (sourceType.Contains("<"))
                    {
                        sourceType = ExtractCleanGenericType(sourceType);
                    }
                }
            }
            
            // If we found a valid source type, add it
            if (!string.IsNullOrEmpty(sourceType) && !IsKeyword(sourceType))
            {
                string pair = $"{sourceType}|{destType}";
                if (seen.Add(pair))
                {
                    results.Add(pair);
                }
            }
            else
            {
                // DEBUG: Track unresolved expressions
                // Could add diagnostic here if needed for debugging
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
        var newPattern = @"(new\s+[a-zA-Z_][\w<>,]*?\s*\([^\)]*\))\s*$";
        var newMatch = Regex.Match(segment, newPattern);
        if (newMatch.Success)
        {
            return newMatch.Groups[1].Value.Trim();
        }
        
        // Pattern 2: identifier (variable, property access, method call)
        var identPattern = @"([a-zA-Z_][\w\.]*(?:\([^\)]*\))?)\s*$";
        var identMatch = Regex.Match(segment, identPattern);
        if (identMatch.Success)
        {
            return identMatch.Groups[1].Value.Trim();
        }
        
        return string.Empty;
    }
    
    private static string ExtractTypeWithinAngleBrackets(string content, int startPos)
    {
        int depth = 1;
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
        var keywords = new[] { "var", "int", "string", "bool", "double", "float", "long", "short", "byte", "char", "object", "dynamic", "void" };
        for (int i = 0; i < keywords.Length; i++)
        {
            if (keywords[i] == word)
                return true;
        }
        return false;
    }
}
