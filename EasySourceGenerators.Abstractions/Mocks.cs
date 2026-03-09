namespace EasySourceGenerators.Abstractions;

public class MockGeneratorsFactory : IGeneratorsFactory
{
    public IMethodBodyBuilder ForMethod() => new MockMethodBodyBuilder();

    public IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>() => new MockMethodImplementationGenerator<TReturnType>();

    public IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>() =>
        new MockMethodImplementationGenerator<TArg1, TReturnType>();
}

public class MockMethodImplementationGenerator<TReturnType> : IMethodImplementationGenerator<TReturnType>
{
    public IMethodImplementationGenerator UseBody(Func<object> body) => this;
}

public class MockMethodImplementationGenerator<TArg1, TReturnType> : IMethodImplementationGenerator<TArg1, TReturnType>
{
    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody() =>
        new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> : IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>
{
    public IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params object[] cases)
        => new MockMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>();

    public IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
        => new MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> : IMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1,
    TReturnType>
{
    public IMethodImplementationGenerator<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> func)
        => new MockMethodImplementationGenerator<TArg1, TReturnType>();

    public IMethodImplementationGenerator<TArg1, TReturnType> UseBody(Func<TArg1, Func<TReturnType>> func)
        => new MockMethodImplementationGenerator<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType> : IMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory)
        => new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();

    public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> UseBody(Func<TArg1, Action<TReturnType>> body)
        => new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
}

public class MockMethodBodyBuilder : IMethodBodyBuilder
{
    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MockMethodBodyBuilder<TArg1>();

    public IMethodImplementationGenerator<TReturnType> WithReturnType<TReturnType>() => new MockImplementationGenerator<TReturnType>();
}

public class MockImplementationGenerator<TReturnType> : IMethodImplementationGenerator<TReturnType>
{
    public IMethodImplementationGenerator UseBody(Func<object> body) => this;
}

public class MockMethodBodyBuilder<TArg1Input> : IMethodBodyBuilder<TArg1Input>
{
    public IMethodImplementationGenerator<TArg1Input, TReturnType> WithReturnType<TReturnType>()
        => new MockMethodImplementationGenerator<TArg1Input, TReturnType>();
}