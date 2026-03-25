using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Resolves the EasySourceGenerators.Abstractions assembly from compilation references.
/// Handles both file-based (<see cref="PortableExecutableReference"/>) and in-memory
/// (<see cref="CompilationReference"/>) references, such as those provided by Rider's code inspector.
/// </summary>
internal static class AbstractionsAssemblyResolver
{
    /// <summary>
    /// Resolves the abstractions assembly from the compilation references into the given
    /// <see cref="LoadedAssemblyContext"/>.
    /// </summary>
    internal static (Assembly? assembly, string? error) Resolve(
        LoadedAssemblyContext context,
        Compilation compilation)
    {
        MetadataReference[] matchingReferences = FindAbstractionsReferences(compilation);

        if (matchingReferences.Length == 0)
        {
            return (null, BuildNoMatchError(compilation));
        }

        PortableExecutableReference[] peReferences =
            matchingReferences.OfType<PortableExecutableReference>().ToArray();
        CompilationReference[] compilationReferences =
            matchingReferences.OfType<CompilationReference>().ToArray();

        if (peReferences.Length > 0)
        {
            return LoadFromPortableExecutableReference(peReferences.First(), context);
        }

        if (compilationReferences.Length > 0)
        {
            return LoadFromCompilationReference(context);
        }

        string matchesString = string.Join(", ",
            matchingReferences.Select(reference =>
                $"{reference.Display} (type: {reference.GetType().Name})"));
        return (null,
            $"Found references matching '{Consts.AbstractionsAssemblyName}' but none were PortableExecutableReference or CompilationReference with valid file paths. \nMatching references: {matchesString}");
    }

    /// <summary>
    /// Finds all compilation references that match the abstractions assembly name.
    /// </summary>
    private static MetadataReference[] FindAbstractionsReferences(Compilation compilation)
    {
        return compilation.References.Where(reference =>
                reference.Display is not null && (
                    reference.Display.Equals(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase)
                    || (reference is PortableExecutableReference peRef && peRef.FilePath is not null &&
                        Path.GetFileNameWithoutExtension(peRef.FilePath)
                            .Equals(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase))))
            .ToArray();
    }

    /// <summary>
    /// Builds an error message when no matching abstractions reference is found.
    /// </summary>
    private static string BuildNoMatchError(Compilation compilation)
    {
        MetadataReference[] closestMatches = compilation.References.Where(reference =>
                reference.Display is not null
                && reference.Display.Contains(Consts.SolutionNamespace, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        string closestMatchesString = string.Join(", ", closestMatches.Select(reference => reference.Display));

        return $"Could not find any reference matching '{Consts.AbstractionsAssemblyName}' in compilation references.\n" +
               $" Found total references: {compilation.References.Count()}. \nMatching references: {closestMatches.Length}: \n{closestMatchesString}";
    }

    /// <summary>
    /// Loads the abstractions assembly from a file-based <see cref="PortableExecutableReference"/>.
    /// </summary>
    private static (Assembly? assembly, string? error) LoadFromPortableExecutableReference(
        PortableExecutableReference reference,
        LoadedAssemblyContext context)
    {
        if (string.IsNullOrEmpty(reference.FilePath))
        {
            return (null,
                $"The reference matching '{Consts.AbstractionsAssemblyName}' does not have a valid file path.");
        }

        string assemblyPath = GeneratorAssemblyExecutor.ResolveImplementationAssemblyPath(reference.FilePath);
        Assembly assembly = context.LoadContext.LoadFromAssemblyPath(assemblyPath);
        return (assembly, null);
    }

    /// <summary>
    /// Loads the abstractions assembly from an in-memory <see cref="CompilationReference"/>.
    /// Uses the captured assembly from the load context if available, otherwise emits from bytes.
    /// </summary>
    private static (Assembly? assembly, string? error) LoadFromCompilationReference(
        LoadedAssemblyContext context)
    {
        if (context.CapturedAbstractionsAssembly != null)
        {
            return (context.CapturedAbstractionsAssembly, null);
        }

        if (context.CompilationReferenceBytes.TryGetValue(Consts.AbstractionsAssemblyName,
                out byte[]? abstractionBytes))
        {
            Assembly assembly = context.LoadContext.LoadFromStream(new MemoryStream(abstractionBytes));
            return (assembly, null);
        }

        return (null,
            $"Found reference matching '{Consts.AbstractionsAssemblyName}' as a CompilationReference, but failed to emit it to a loadable assembly.");
    }
}
