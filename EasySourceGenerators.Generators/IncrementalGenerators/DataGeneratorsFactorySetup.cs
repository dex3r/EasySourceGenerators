using System;
using System.Reflection;

namespace EasySourceGenerators.Generators.IncrementalGenerators;

/// <summary>
/// Sets up the <c>DataGeneratorsFactory</c> in the loaded execution assembly and
/// assigns it to <c>Generate.CurrentGenerator</c> in the abstractions assembly,
/// enabling fluent API usage during generator execution.
/// </summary>
internal static class DataGeneratorsFactorySetup
{
    /// <summary>
    /// Creates a <c>DataGeneratorsFactory</c> instance and wires it to the
    /// <c>Generate.CurrentGenerator</c> static property. Returns an error message
    /// if the required types or properties cannot be found.
    /// </summary>
    internal static string? Setup(
        Assembly executionAssembly,
        Assembly abstractionsAssembly)
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
