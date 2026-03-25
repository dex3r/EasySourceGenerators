using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Provides helper methods for formatting diagnostic and error messages
/// used during generator execution.
/// </summary>
internal static class DiagnosticMessageHelper
{
    /// <summary>
    /// Joins error diagnostics from a compilation result into a single semicolon-separated string.
    /// Only includes diagnostics with <see cref="DiagnosticSeverity.Error"/> severity.
    /// </summary>
    internal static string JoinErrorDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        return string.Join("; ", diagnostics
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => diagnostic.GetMessage()));
    }
}
