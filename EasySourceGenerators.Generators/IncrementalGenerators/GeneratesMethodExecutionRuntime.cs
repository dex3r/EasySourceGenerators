using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

internal sealed record SwitchBodyData(
    IReadOnlyList<(object key, string value)> CasePairs,
    bool HasDefaultCase);

/// <summary>
/// Result extracted from <see cref="DataBuilding.BodyGenerationData"/> after executing a fluent body generator method.
/// </summary>
internal sealed record FluentBodyResult(
    string? ReturnValue,
    bool IsVoid);

/// <summary>
/// Orchestrates the execution of generator methods at compile time.
/// Delegates compilation and assembly loading to <see cref="GeneratorAssemblyExecutor"/>,
/// and data extraction to <see cref="BodyGenerationDataExtractor"/>.
/// </summary>
internal static class GeneratesMethodExecutionRuntime
{
    /// <summary>
    /// Executes a simple (non-fluent) generator method with no arguments and returns its string result.
    /// </summary>
    internal static (string? value, string? error) ExecuteSimpleGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        IReadOnlyList<IMethodSymbol> allPartials = GetAllUnimplementedPartialMethods(compilation);
        return ExecuteGeneratorMethodWithArgs(generatorMethod, allPartials, compilation, null);
    }

    /// <summary>
    /// Executes a fluent body generator method and extracts the <see cref="FluentBodyResult"/>
    /// from the returned <c>DataMethodBodyGenerator</c>.
    /// </summary>
    internal static (FluentBodyResult? result, string? error) ExecuteFluentBodyGeneratorMethod(
        IMethodSymbol generatorMethod,
        IMethodSymbol partialMethod,
        Compilation compilation)
    {
        IReadOnlyList<IMethodSymbol> allPartials = GetAllUnimplementedPartialMethods(compilation);

        (LoadedAssemblyContext? loadedContext, string? loadError) =
            GeneratorAssemblyExecutor.CompileAndLoadAssembly(allPartials, compilation);
        if (loadError != null)
        {
            return (null, loadError);
        }

        using LoadedAssemblyContext context = loadedContext!;
        try
        {
            (Assembly? abstractionsAssembly, string? abstractionsError) =
                ResolveAbstractionsAssembly(context, compilation);
            if (abstractionsError != null)
            {
                return (null, abstractionsError);
            }

            string? setupError = SetupDataGeneratorsFactory(context.Assembly, abstractionsAssembly!, context);
            if (setupError != null)
            {
                return (null, setupError);
            }

            string typeName = generatorMethod.ContainingType.ToDisplayString();
            (Type? loadedType, string? typeError) = GeneratorAssemblyExecutor.FindType(context.Assembly, typeName);
            if (typeError != null)
            {
                return (null, typeError);
            }

            (MethodInfo? methodInfo, string? methodError) =
                GeneratorAssemblyExecutor.FindStaticMethod(loadedType!, generatorMethod.Name, typeName);
            if (methodError != null)
            {
                return (null, methodError);
            }

            object? methodResult = methodInfo!.Invoke(null, null);
            if (methodResult == null)
            {
                return (null, "Fluent body generator method returned null");
            }

            bool isVoidReturnType = partialMethod.ReturnType.SpecialType == SpecialType.System_Void;
            FluentBodyResult bodyResult = BodyGenerationDataExtractor.Extract(methodResult, isVoidReturnType);
            return (bodyResult, null);
        }
        catch (Exception ex)
        {
            return (null, $"Error executing generator method '{generatorMethod.Name}': {ex.GetBaseException()}");
        }
    }

    /// <summary>
    /// Executes a generator method with optional arguments and returns its string result.
    /// </summary>
    internal static (string? value, string? error) ExecuteGeneratorMethodWithArgs(
        IMethodSymbol generatorMethod,
        IReadOnlyList<IMethodSymbol> allPartialMethods,
        Compilation compilation,
        object?[]? args)
    {
        (LoadedAssemblyContext? loadedContext, string? loadError) =
            GeneratorAssemblyExecutor.CompileAndLoadAssembly(allPartialMethods, compilation);
        if (loadError != null)
        {
            return (null, loadError);
        }

        using LoadedAssemblyContext context = loadedContext!;
        try
        {
            string typeName = generatorMethod.ContainingType.ToDisplayString();
            (Type? loadedType, string? typeError) = GeneratorAssemblyExecutor.FindType(context.Assembly, typeName);
            if (typeError != null)
            {
                return (null, typeError);
            }

            (MethodInfo? methodInfo, string? methodError) =
                GeneratorAssemblyExecutor.FindStaticMethod(loadedType!, generatorMethod.Name, typeName);
            if (methodError != null)
            {
                return (null, methodError);
            }

            object?[]? convertedArgs = GeneratorAssemblyExecutor.ConvertArguments(args, methodInfo!);
            object? result = methodInfo!.Invoke(null, convertedArgs);
            return (result?.ToString(), null);
        }
        catch (Exception ex)
        {
            return (null, $"Error executing generator method '{generatorMethod.Name}': {ex.GetBaseException()}");
        }
    }

    /// <summary>
    /// Finds all unimplemented partial method definitions across all syntax trees in the compilation.
    /// </summary>
    internal static IReadOnlyList<IMethodSymbol> GetAllUnimplementedPartialMethods(Compilation compilation)
    {
        List<IMethodSymbol> methods = new();
        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            IEnumerable<MethodDeclarationSyntax> partialMethodDeclarations = syntaxTree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)));

            foreach (MethodDeclarationSyntax declaration in partialMethodDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(declaration) is IMethodSymbol symbol && symbol.IsPartialDefinition)
                {
                    methods.Add(symbol);
                }
            }
        }

        return methods;
    }

    /// <summary>
    /// Resolves the abstractions assembly from the compilation references.
    /// Handles both <see cref="PortableExecutableReference"/> (file-based) and
    /// <see cref="CompilationReference"/> (in-memory, e.g., from Rider's code inspector).
    /// </summary>
    private static (Assembly? assembly, string? error) ResolveAbstractionsAssembly(
        LoadedAssemblyContext context,
        Compilation compilation)
    {
        MetadataReference[] abstractionsMatchingReferences = compilation.References.Where(reference =>
                reference.Display is not null && (
                    reference.Display.Equals(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase)
                    || (reference is PortableExecutableReference peRef && peRef.FilePath is not null &&
                        Path.GetFileNameWithoutExtension(peRef.FilePath)
                            .Equals(Consts.AbstractionsAssemblyName, StringComparison.OrdinalIgnoreCase))))
            .ToArray();

        if (abstractionsMatchingReferences.Length == 0)
        {
            MetadataReference[] closestMatches = compilation.References.Where(reference =>
                    reference.Display is not null
                    && reference.Display.Contains(Consts.SolutionNamespace, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            string closestMatchesString = string.Join(", ", closestMatches.Select(reference => reference.Display));

            return (null,
                $"Could not find any reference matching '{Consts.AbstractionsAssemblyName}' in compilation references.\n" +
                $" Found total references: {compilation.References.Count()}. \nMatching references: {closestMatches.Length}: \n{closestMatchesString}");
        }

        PortableExecutableReference[] peMatchingReferences =
            abstractionsMatchingReferences.OfType<PortableExecutableReference>().ToArray();
        CompilationReference[] csharpCompilationReference =
            abstractionsMatchingReferences.OfType<CompilationReference>().ToArray();

        if (peMatchingReferences.Length > 0)
        {
            PortableExecutableReference abstractionsReference = peMatchingReferences.First();

            if (string.IsNullOrEmpty(abstractionsReference.FilePath))
            {
                return (null,
                    $"The reference matching '{Consts.AbstractionsAssemblyName}' does not have a valid file path.");
            }

            string abstractionsAssemblyPath =
                GeneratorAssemblyExecutor.ResolveImplementationAssemblyPath(abstractionsReference.FilePath);
            Assembly abstractionsAssembly = context.LoadContext.LoadFromAssemblyPath(abstractionsAssemblyPath);
            return (abstractionsAssembly, null);
        }

        if (csharpCompilationReference.Length > 0)
        {
            if (context.CapturedAbstractionsAssembly != null)
            {
                return (context.CapturedAbstractionsAssembly, null);
            }

            if (context.CompilationReferenceBytes.TryGetValue(Consts.AbstractionsAssemblyName,
                    out byte[]? abstractionBytes))
            {
                Assembly abstractionsAssembly =
                    context.LoadContext.LoadFromStream(new MemoryStream(abstractionBytes));
                return (abstractionsAssembly, null);
            }

            return (null,
                $"Found reference matching '{Consts.AbstractionsAssemblyName}' as a CompilationReference, but failed to emit it to a loadable assembly.");
        }

        string matchesString = string.Join(", ",
            abstractionsMatchingReferences.Select(reference =>
                $"{reference.Display} (type: {reference.GetType().Name})"));
        return (null,
            $"Found references matching '{Consts.AbstractionsAssemblyName}' but none were PortableExecutableReference or CompilationReference with valid file paths. \nMatching references: {matchesString}");
    }

    /// <summary>
    /// Sets up the <c>DataGeneratorsFactory</c> and assigns it to <c>Generate.CurrentGenerator</c>
    /// in the loaded abstractions assembly, enabling fluent API usage during generator execution.
    /// </summary>
    private static string? SetupDataGeneratorsFactory(
        Assembly executionAssembly,
        Assembly abstractionsAssembly,
        LoadedAssemblyContext context)
    {
        Type? generatorStaticType = abstractionsAssembly.GetType(Consts.GenerateTypeFullName);
        Type? dataGeneratorsFactoryType = executionAssembly.GetType(Consts.DataGeneratorsFactoryTypeFullName);
        if (generatorStaticType == null || dataGeneratorsFactoryType == null)
        {
            return
                $"Could not find {Consts.GenerateTypeFullName} or {Consts.DataGeneratorsFactoryTypeFullName} types in compiled assembly";
        }

        object? dataGeneratorsFactory = Activator.CreateInstance(dataGeneratorsFactoryType);
        PropertyInfo? currentGeneratorProperty = generatorStaticType.GetProperty(
            Consts.CurrentGeneratorPropertyName, BindingFlags.NonPublic | BindingFlags.Static);
        currentGeneratorProperty?.SetValue(null, dataGeneratorsFactory);

        return null;
    }
}
