using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Extracts the delegate body source code from a <c>UseProvidedBody(...)</c> invocation
/// within a generator method's syntax tree. The extracted body is re-indented to match
/// the target method body indentation (8 spaces).
/// </summary>
internal static class DelegateBodySyntaxExtractor
{
    private const string MethodBodyIndent = "        ";

    /// <summary>
    /// Attempts to find a <c>UseProvidedBody(...)</c> call in the given generator method syntax
    /// and extract the lambda body. Returns <c>null</c> if no such call is found.
    /// For expression lambdas, returns a single <c>return {expr};</c> line.
    /// For block lambdas, returns the block body re-indented to the method body level.
    /// </summary>
    internal static string? TryExtractDelegateBody(MethodDeclarationSyntax generatorMethodSyntax)
    {
        InvocationExpressionSyntax? invocation = generatorMethodSyntax
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(inv =>
                inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "UseProvidedBody");

        if (invocation == null)
        {
            return null;
        }

        ArgumentSyntax? argument = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (argument?.Expression is not LambdaExpressionSyntax lambda)
        {
            return null;
        }

        if (lambda.Body is ExpressionSyntax expression)
        {
            string expressionText = expression.ToFullString().Trim();
            return expressionText;
        }

        if (lambda.Body is BlockSyntax block)
        {
            return ExtractBlockBody(block);
        }

        return null;
    }

    /// <summary>
    /// Extracts the content of a block body (between <c>{</c> and <c>}</c>),
    /// determines the base indentation, and re-indents all lines to the method body level.
    /// Blank lines between statements are preserved with method body indentation.
    /// </summary>
    private static string? ExtractBlockBody(BlockSyntax block)
    {
        string blockText = block.ToFullString();
        string[] lines = blockText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        int openIndex = -1;
        int closeIndex = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            if (openIndex == -1 && lines[i].TrimEnd().EndsWith("{", StringComparison.Ordinal))
            {
                openIndex = i;
                break;
            }
        }

        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.StartsWith("}", StringComparison.Ordinal))
            {
                closeIndex = i;
                break;
            }
        }

        if (openIndex == -1 || closeIndex == -1 || closeIndex <= openIndex)
        {
            return null;
        }

        string[] contentLines = new string[closeIndex - openIndex - 1];
        Array.Copy(lines, openIndex + 1, contentLines, 0, contentLines.Length);

        if (contentLines.Length == 0)
        {
            return null;
        }

        int minIndent = int.MaxValue;
        foreach (string line in contentLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            int indent = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    indent++;
                }
                else if (c == '\t')
                {
                    indent += 4;
                }
                else
                {
                    break;
                }
            }

            if (indent < minIndent)
            {
                minIndent = indent;
            }
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        StringBuilder result = new();
        for (int i = 0; i < contentLines.Length; i++)
        {
            string line = contentLines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                result.AppendLine(MethodBodyIndent);
            }
            else
            {
                string stripped = minIndent <= line.Length ? line.Substring(minIndent) : line.TrimStart();
                string trimmedEnd = stripped.TrimEnd();
                result.AppendLine(MethodBodyIndent + trimmedEnd);
            }
        }

        return result.ToString().TrimEnd('\n', '\r');
    }
}
