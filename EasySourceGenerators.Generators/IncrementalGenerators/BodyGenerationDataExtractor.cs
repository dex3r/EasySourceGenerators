using System;
using System.Reflection;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Extracts <see cref="FluentBodyResult"/> data from a method result object via reflection.
/// The result object is expected to be a <c>DataMethodBodyGenerator</c> containing a
/// <c>BodyGenerationData</c> property, from which return values and delegate bodies are extracted.
/// </summary>
internal static class BodyGenerationDataExtractor
{
    /// <summary>
    /// Extracts the return value from a fluent body generator method result using reflection.
    /// Checks for <c>ReturnConstantValueFactory</c> first, then <c>RuntimeDelegateBody</c>.
    /// Returns a <see cref="FluentBodyResult"/> with the extracted value, or <c>null</c> return value
    /// if neither factory nor body are present.
    /// Sets <see cref="FluentBodyResult.HasDelegateBody"/> when <c>RuntimeDelegateBody</c> is present,
    /// indicating that the delegate body source code should be extracted from the syntax tree.
    /// </summary>
    internal static FluentBodyResult Extract(object methodResult, bool isVoidReturnType)
    {
        Type resultType = methodResult.GetType();

        // The result should be a DataMethodBodyGenerator containing a BodyGenerationData Data property
        PropertyInfo? dataProperty = resultType.GetProperty(Consts.BodyGenerationDataPropertyName);
        if (dataProperty == null)
        {
            // The method returned something that isn't a DataMethodBodyGenerator.
            // This may happen when the fluent chain is incomplete (e.g., user returned an intermediate builder).
            return new FluentBodyResult(null, isVoidReturnType, HasDelegateBody: false);
        }

        object? bodyGenerationData = dataProperty.GetValue(methodResult);
        if (bodyGenerationData == null)
        {
            return new FluentBodyResult(null, isVoidReturnType, HasDelegateBody: false);
        }

        Type dataType = bodyGenerationData.GetType();
        PropertyInfo? returnTypeProperty = dataType.GetProperty("ReturnType");
        Type? dataReturnType = returnTypeProperty?.GetValue(bodyGenerationData) as Type;
        bool isVoid = dataReturnType == typeof(void);

        bool hasDelegateBody = HasRuntimeDelegateBody(dataType, bodyGenerationData);
        object? compileTimeConstants = GetCompileTimeConstants(dataType, bodyGenerationData);

        // When compile-time constants are present, the delegate body references a 'constants' parameter
        // that doesn't exist in the generated method, so syntax extraction cannot be used.
        if (compileTimeConstants != null)
        {
            hasDelegateBody = false;
        }

        return TryExtractFromConstantFactory(dataType, bodyGenerationData, isVoid, compileTimeConstants)
               ?? TryExtractFromRuntimeBody(dataType, bodyGenerationData, isVoid, hasDelegateBody, compileTimeConstants)
               ?? new FluentBodyResult(null, isVoid, hasDelegateBody);
    }

    /// <summary>
    /// Checks whether <c>RuntimeDelegateBody</c> is set (non-null) in the body generation data.
    /// </summary>
    private static bool HasRuntimeDelegateBody(Type dataType, object bodyGenerationData)
    {
        PropertyInfo? runtimeBodyProperty = dataType.GetProperty("RuntimeDelegateBody");
        Delegate? runtimeBody = runtimeBodyProperty?.GetValue(bodyGenerationData) as Delegate;
        return runtimeBody != null;
    }

    /// <summary>
    /// Retrieves the <c>CompileTimeConstants</c> value from the body generation data, if present.
    /// </summary>
    private static object? GetCompileTimeConstants(Type dataType, object bodyGenerationData)
    {
        PropertyInfo? constantsProperty = dataType.GetProperty("CompileTimeConstants");
        return constantsProperty?.GetValue(bodyGenerationData);
    }

    /// <summary>
    /// Attempts to extract a return value by invoking the <c>ReturnConstantValueFactory</c> delegate.
    /// If <paramref name="compileTimeConstants"/> is provided and the factory accepts a parameter,
    /// the constants are passed as the first argument.
    /// </summary>
    private static FluentBodyResult? TryExtractFromConstantFactory(
        Type dataType,
        object bodyGenerationData,
        bool isVoid,
        object? compileTimeConstants)
    {
        PropertyInfo? constantFactoryProperty = dataType.GetProperty("ReturnConstantValueFactory");
        Delegate? constantFactory = constantFactoryProperty?.GetValue(bodyGenerationData) as Delegate;
        if (constantFactory == null)
        {
            return null;
        }

        ParameterInfo[] factoryParams = constantFactory.Method.GetParameters();
        object? constantValue;

        if (factoryParams.Length == 1 && compileTimeConstants != null)
        {
            constantValue = constantFactory.DynamicInvoke(compileTimeConstants);
        }
        else if (factoryParams.Length == 0)
        {
            constantValue = constantFactory.DynamicInvoke();
        }
        else
        {
            return null;
        }

        return new FluentBodyResult(constantValue?.ToString(), isVoid, HasDelegateBody: false);
    }

    /// <summary>
    /// Attempts to extract a return value by invoking the <c>RuntimeDelegateBody</c> delegate.
    /// If the delegate has no parameters, it is invoked directly.
    /// If <paramref name="compileTimeConstants"/> is provided and the delegate has exactly one parameter
    /// (the constants), it is invoked with the constants. Delegates with additional parameters
    /// (method parameters) cannot be executed at compile time without concrete values.
    /// </summary>
    private static FluentBodyResult? TryExtractFromRuntimeBody(
        Type dataType,
        object bodyGenerationData,
        bool isVoid,
        bool hasDelegateBody,
        object? compileTimeConstants)
    {
        PropertyInfo? runtimeBodyProperty = dataType.GetProperty("RuntimeDelegateBody");
        Delegate? runtimeBody = runtimeBodyProperty?.GetValue(bodyGenerationData) as Delegate;
        if (runtimeBody == null)
        {
            return null;
        }

        ParameterInfo[] bodyParams = runtimeBody.Method.GetParameters();
        if (bodyParams.Length == 0)
        {
            object? bodyResult = runtimeBody.DynamicInvoke();
            return new FluentBodyResult(bodyResult?.ToString(), isVoid, hasDelegateBody);
        }

        if (bodyParams.Length == 1 && compileTimeConstants != null)
        {
            object? bodyResult = runtimeBody.DynamicInvoke(compileTimeConstants);
            return new FluentBodyResult(bodyResult?.ToString(), isVoid, hasDelegateBody);
        }

        // For delegates with additional parameters, we can't invoke at compile time without values
        return new FluentBodyResult(null, isVoid, hasDelegateBody);
    }
}
