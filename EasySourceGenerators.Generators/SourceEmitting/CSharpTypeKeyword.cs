using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Maps Roslyn <see cref="TypeKind"/> values to their C# type-declaration keyword.
/// </summary>
internal static class CSharpTypeKeyword
{
    /// <summary>
    /// Returns the C# keyword (<c>"class"</c>, <c>"struct"</c>, or <c>"interface"</c>)
    /// for the given type kind. Returns <c>"class"</c> for unrecognized values.
    /// </summary>
    internal static string From(TypeKind typeKind)
    {
        return typeKind switch
        {
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            _ => "class"
        };
    }
}
