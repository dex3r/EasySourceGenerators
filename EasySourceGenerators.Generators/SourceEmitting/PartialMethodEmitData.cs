namespace EasySourceGenerators.Generators.SourceEmitting;

/// <summary>
/// Contains all data needed to emit a partial method implementation source file,
/// decoupled from Roslyn symbol types for easier unit testing.
/// </summary>
internal sealed record PartialMethodEmitData(
    string GeneratorFullName,
    string? NamespaceName,
    string TypeName,
    string TypeKeyword,
    string TypeModifiers,
    string AccessibilityKeyword,
    string MethodModifiers,
    string ReturnTypeName,
    string MethodName,
    string ParameterList,
    bool ReturnsVoid);
