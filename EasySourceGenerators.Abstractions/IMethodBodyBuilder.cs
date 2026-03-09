namespace EasySourceGenerators.Abstractions;

public interface IMethodBodyBuilder
{
    IMethodBodyBuilder<TArg1> WithParameter<TArg1>();
    IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBodyBuilder<TArg1>
{
    IMethodImplementationGenerator<TArg1, TReturnType> WithReturnType<TReturnType>();
}