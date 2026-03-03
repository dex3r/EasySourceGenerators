namespace MattSourceGenHelpers.Abstractions;

public static class Generate
{
    internal static IGeneratorsFactory CurrentGenerator { get; set; } = new RecordingGeneratorsFactory();

    public static IMethodBuilder Method() => new MethodBuilder(CurrentGenerator);
}

public interface IMethodBuilder
{
    IMethodBuilder<TArg1> WithParameter<TArg1>();
    IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBuilder<TArg1>
{
    IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>();
}

public class MethodBuilder(IGeneratorsFactory generatorsFactory) : IMethodBuilder
{
    public IMethodBuilder<TArg1> WithParameter<TArg1>() => new MethodBuilder<TArg1>(generatorsFactory);

    public IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBuilder<TArg1>(IGeneratorsFactory generatorsFactory) : IMethodBuilder<TArg1>
{
    public IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TArg1, TReturnType>();
}

public interface IGeneratorsFactory
{
    IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>();
    IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>();
}
