using System.Linq;
using EasySourceGenerators.Generators.SourceEmitting;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Builds C# source code for partial method implementations by delegating to
/// the <see cref="SourceEmitting"/> classes for string emission and literal formatting.
/// </summary>
internal static class GeneratesMethodPatternSourceBuilder
{
    /// <summary>
    /// Generates a complete C# source file containing a simple partial method implementation
    /// that returns the given value, formatted as a C# literal.
    /// </summary>
    internal static string GenerateSimplePartialMethod(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        string? returnValue)
    {
        PartialMethodEmitData emitData = RoslynSymbolDataMapper.ToPartialMethodEmitData(containingType, partialMethod);

        string? returnValueLiteral = !partialMethod.ReturnsVoid
            ? FormatValueAsCSharpLiteral(returnValue, partialMethod.ReturnType)
            : null;

        return PartialMethodSourceEmitter.Emit(emitData, returnValueLiteral);
    }

    /// <summary>
    /// Generates a complete C# source file containing a partial method implementation
    /// with the given body lines (already indented to the method body level).
    /// </summary>
    internal static string GeneratePartialMethodWithBody(
        INamedTypeSymbol containingType,
        IMethodSymbol partialMethod,
        string bodyLines)
    {
        PartialMethodEmitData emitData = RoslynSymbolDataMapper.ToPartialMethodEmitData(containingType, partialMethod);
        return PartialMethodSourceEmitter.EmitWithBody(emitData, bodyLines);
    }

    /// <summary>
    /// Formats a string value as a C# literal expression based on the target return type.
    /// Delegates to <see cref="CSharpLiteralFormatter.FormatValueAsLiteral"/>.
    /// </summary>
    internal static string FormatValueAsCSharpLiteral(string? value, ITypeSymbol returnType)
    {
        return CSharpLiteralFormatter.FormatValueAsLiteral(
            value,
            returnType.SpecialType,
            returnType.TypeKind,
            returnType.ToDisplayString());
    }

    /// <summary>
    /// Formats a key object as a C# literal expression for use in switch case labels.
    /// Delegates to <see cref="CSharpLiteralFormatter.FormatKeyAsLiteral"/>.
    /// </summary>
    internal static string FormatKeyAsCSharpLiteral(object key, ITypeSymbol? parameterType)
    {
        return CSharpLiteralFormatter.FormatKeyAsLiteral(
            key,
            parameterType?.TypeKind,
            parameterType?.ToDisplayString());
    }
}
