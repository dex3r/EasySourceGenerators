using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Maps Roslyn <see cref="Accessibility"/> values to their C# keyword representations.
/// </summary>
internal static class CSharpAccessibilityKeyword
{
    /// <summary>
    /// Returns the C# keyword for the given accessibility level.
    /// Returns <c>"private"</c> for <see cref="Accessibility.NotApplicable"/> and unrecognized values.
    /// </summary>
    internal static string From(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };
    }

    /// <summary>
    /// Returns the C# keyword for the given accessibility level, or an empty string
    /// for <see cref="Accessibility.Private"/> and unrecognized values.
    /// Used in contexts where the <c>private</c> keyword is implicit (e.g., dummy implementations).
    /// </summary>
    internal static string FromOrEmpty(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => ""
        };
    }
}
