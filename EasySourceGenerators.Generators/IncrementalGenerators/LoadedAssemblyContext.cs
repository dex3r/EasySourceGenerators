using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Holds the result of compiling and loading a generator assembly in an isolated context.
/// Implements <see cref="IDisposable"/> to ensure the <see cref="AssemblyLoadContext"/> is unloaded.
/// </summary>
internal sealed class LoadedAssemblyContext : IDisposable
{
    internal Assembly Assembly { get; }
    internal AssemblyLoadContext LoadContext { get; }
    internal Dictionary<string, byte[]> CompilationReferenceBytes { get; }

    /// <summary>
    /// The abstractions assembly loaded during assembly resolution, if it was resolved
    /// from a <see cref="CompilationReference"/>. May be <c>null</c> if the abstractions
    /// assembly was loaded from a file path instead.
    /// </summary>
    internal Assembly? CapturedAbstractionsAssembly { get; }

    internal LoadedAssemblyContext(
        Assembly assembly,
        AssemblyLoadContext loadContext,
        Dictionary<string, byte[]> compilationReferenceBytes,
        Assembly? capturedAbstractionsAssembly)
    {
        Assembly = assembly;
        LoadContext = loadContext;
        CompilationReferenceBytes = compilationReferenceBytes;
        CapturedAbstractionsAssembly = capturedAbstractionsAssembly;
    }

    /// <summary>
    /// Unloads the isolated <see cref="AssemblyLoadContext"/> and all assemblies loaded within it.
    /// </summary>
    public void Dispose()
    {
        LoadContext.Unload();
    }
}
