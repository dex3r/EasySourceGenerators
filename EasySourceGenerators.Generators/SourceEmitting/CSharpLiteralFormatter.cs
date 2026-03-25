using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Formats values as C# literal expressions suitable for source-generated code.
/// </summary>
internal static class CSharpLiteralFormatter
{
    /// <summary>
    /// Formats a string value as a C# literal expression based on the target return type.
    /// Returns <c>"default"</c> when <paramref name="value"/> is <c>null</c>.
    /// </summary>
    internal static string FormatValueAsLiteral(
        string? value,
        SpecialType specialType,
        TypeKind typeKind,
        string typeDisplayString)
    {
        if (value == null)
        {
            return "default";
        }

        return specialType switch
        {
            SpecialType.System_String => SyntaxFactory.Literal(value).Text,
            SpecialType.System_Char when value.Length == 1 => SyntaxFactory.Literal(value[0]).Text,
            SpecialType.System_Boolean => value.ToLowerInvariant(),
            _ when typeKind == TypeKind.Enum => $"{typeDisplayString}.{value}",
            _ => value
        };
    }

    /// <summary>
    /// Formats a key object as a C# literal expression for use in switch case labels.
    /// </summary>
    internal static string FormatKeyAsLiteral(object key, TypeKind? typeKind, string? typeDisplayString)
    {
        if (typeKind == TypeKind.Enum)
        {
            return $"{typeDisplayString}.{key}";
        }

        return key switch
        {
            bool b => b ? "true" : "false",
            // SyntaxFactory.Literal handles escaping and quoting (e.g. "hello" → "\"hello\"")
            string s => SyntaxFactory.Literal(s).Text,
            _ => key.ToString()!
        };
    }
}
