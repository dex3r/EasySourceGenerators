using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using EasySourceGenerators.Generators.SourceEmitting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

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

/// <summary>
/// Handles the compilation, assembly loading, and method invocation steps
/// required to execute generator methods at compile time in an isolated context.
/// </summary>
internal static class GeneratorAssemblyExecutor
{
    /// <summary>
    /// Compiles the generator source code and loads the resulting assembly in an isolated
    /// <see cref="AssemblyLoadContext"/>. Returns a <see cref="LoadedAssemblyContext"/>
    /// that must be disposed to unload the context.
    /// </summary>
    internal static (LoadedAssemblyContext? context, string? error) CompileAndLoadAssembly(
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation)
    {
        CSharpCompilation executableCompilation = BuildExecutionCompilation(allPartialMethods, compilation);

        using MemoryStream stream = new();
        EmitResult emitResult = executableCompilation.Emit(stream);
        if (!emitResult.Success)
        {
            string errors = string.Join("; ", emitResult.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(diagnostic => diagnostic.GetMessage()));
            return (null, $"Compilation failed: {errors}");
        }

        stream.Position = 0;
        Dictionary<string, byte[]> compilationReferenceBytes = EmitCompilationReferences(compilation);

        AssemblyLoadContext loadContext = new("__GeneratorExec", isCollectible: true);
        Assembly? capturedAbstractionsAssembly = null;
        loadContext.Resolving += (context, assemblyName) =>
        {
            PortableExecutableReference? match = compilation.References
                .OfType<PortableExecutableReference>()
                .FirstOrDefault(reference => reference.FilePath is not null && string.Equals(
                    Path.GetFileNameWithoutExtension(reference.FilePath),
                    assemblyName.Name,
                    StringComparison.OrdinalIgnoreCase));
            if (match?.FilePath != null)
                return context.LoadFromAssemblyPath(ResolveImplementationAssemblyPath(match.FilePath));

            if (assemblyName.Name != null && compilationReferenceBytes.TryGetValue(assemblyName.Name, out byte[]? bytes))
            {
                Assembly loaded = context.LoadFromStream(new MemoryStream(bytes));
                if (string.Equals(assemblyName.Name, Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase))
                    capturedAbstractionsAssembly = loaded;
                return loaded;
            }

            return null;
        };

        Assembly assembly = loadContext.LoadFromStream(stream);

        return (new LoadedAssemblyContext(assembly, loadContext, compilationReferenceBytes, capturedAbstractionsAssembly), null);
    }

    /// <summary>
    /// Finds a type in the loaded assembly by its full name.
    /// </summary>
    internal static (Type? type, string? error) FindType(Assembly assembly, string typeName)
    {
        Type? loadedType = assembly.GetType(typeName);
        if (loadedType == null)
        {
            return (null, $"Could not find type '{typeName}' in compiled assembly");
        }

        return (loadedType, null);
    }

    /// <summary>
    /// Finds a static method in the given type by name.
    /// </summary>
    internal static (MethodInfo? method, string? error) FindStaticMethod(Type type, string methodName, string typeName)
    {
        MethodInfo? methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (methodInfo == null)
        {
            return (null, $"Could not find method '{methodName}' in type '{typeName}'");
        }

        return (methodInfo, null);
    }

    /// <summary>
    /// Converts argument values to match the target method's parameter types.
    /// Returns <c>null</c> when <paramref name="args"/> is <c>null</c> or the method has no parameters.
    /// </summary>
    internal static object?[]? ConvertArguments(object?[]? args, MethodInfo methodInfo)
    {
        if (args == null || methodInfo.GetParameters().Length == 0)
        {
            return null;
        }

        Type parameterType = methodInfo.GetParameters()[0].ParameterType;
        return new[] { Convert.ChangeType(args[0], parameterType) };
    }

    /// <summary>
    /// Builds a <see cref="CSharpCompilation"/> suitable for executing generator methods,
    /// by adding dummy partial method implementations and embedded data building sources
    /// to the original compilation.
    /// </summary>
    internal static CSharpCompilation BuildExecutionCompilation(
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation)
    {
        IReadOnlyList<DummyTypeGroupData> dummyTypeGroups = RoslynSymbolDataMapper.ToDummyTypeGroups(allPartialMethods);
        string dummySource = DummyImplementationEmitter.Emit(dummyTypeGroups);
        string dataGeneratorsFactorySource = ReadEmbeddedResource($"{Consts.GeneratorsAssemblyName}.DataGeneratorsFactory.cs");
        string dataMethodBodyBuildersSource = ReadEmbeddedResource($"{Consts.GeneratorsAssemblyName}.DataMethodBodyBuilders.cs");
        string dataRecordsSource = ReadEmbeddedResource($"{Consts.GeneratorsAssemblyName}.DataRecords.cs");
        CSharpParseOptions parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                                         ?? CSharpParseOptions.Default;

        return (CSharpCompilation)compilation
            .AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(dummySource, parseOptions),
                CSharpSyntaxTree.ParseText(dataGeneratorsFactorySource, parseOptions),
                CSharpSyntaxTree.ParseText(dataMethodBodyBuildersSource, parseOptions),
                CSharpSyntaxTree.ParseText(dataRecordsSource, parseOptions));
    }

    /// <summary>
    /// Reads an embedded resource from the current assembly by its logical name.
    /// </summary>
    internal static string ReadEmbeddedResource(string resourceName)
    {
        using Stream? stream = typeof(GeneratorAssemblyExecutor).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found in {Consts.GeneratorsAssemblyName} assembly");
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Emits all <see cref="CompilationReference"/>s in the compilation to in-memory byte arrays,
    /// keyed by assembly name. These are used to resolve assemblies during isolated execution.
    /// </summary>
    internal static Dictionary<string, byte[]> EmitCompilationReferences(Compilation compilation)
    {
        Dictionary<string, byte[]> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (CompilationReference compilationRef in compilation.References.OfType<CompilationReference>())
        {
            string assemblyName = compilationRef.Compilation.AssemblyName ?? string.Empty;
            if (string.IsNullOrEmpty(assemblyName))
                continue;
            using MemoryStream refStream = new();
            if (compilationRef.Compilation.Emit(refStream).Success)
                result[assemblyName] = refStream.ToArray();
        }

        return result;
    }

    /// <summary>
    /// Resolves a reference assembly path to its implementation assembly path.
    /// When the path points to a <c>ref/</c> directory, returns the corresponding
    /// implementation assembly one level up.
    /// </summary>
    internal static string ResolveImplementationAssemblyPath(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        string? parentDirectory = directory != null ? Path.GetDirectoryName(directory) : null;
        if (directory != null &&
            parentDirectory != null &&
            string.Equals(Path.GetFileName(directory), "ref", StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine(parentDirectory, Path.GetFileName(path));
        }

        return path;
    }
}
