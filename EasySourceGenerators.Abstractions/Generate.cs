using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

public static class Generate
{
    internal static IGeneratorsFactory CurrentGenerator { get; [UsedImplicitly] set; } = new MockGeneratorsFactory();

    public static IMethodBodyBuilder MethodBody() => CurrentGenerator.ForMethod();
}