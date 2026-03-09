using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Generators;

public class MethodBodyBuilder(IGeneratorsFactory generatorsFactory) : IMethodBodyBuilder
{
    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MethodBodyBodyBuilder<TArg1>(generatorsFactory);

    public IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TReturnType>();
}

public class MethodBodyBodyBuilder<TArg1>(IGeneratorsFactory generatorsFactory) : IMethodBodyBuilder<TArg1>
{
    public IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>() => generatorsFactory.CreateImplementation<TArg1, TReturnType>();
}
