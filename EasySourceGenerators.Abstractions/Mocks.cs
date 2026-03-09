namespace EasySourceGenerators.Abstractions;

public class MockGeneratorsFactory : IGeneratorsFactory
{
    public IMethodBodyBuilder ForMethod() => new MockMethodBodyBuilder();

    public IMethodBodyGenerator<TReturnType> CreateImplementation<TReturnType>() => new MockMethodImplementationGenerator<TReturnType>();

    public IMethodBodyGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>() =>
        new MockMethodImplementationGenerator<TArg1, TReturnType>();
}

public class MockMethodImplementationGenerator<TReturnType> : IMethodBodyGenerator<TReturnType>
{
    public IMethodBodyGenerator BodyReturningConstantValue(Func<object> body) => this;
}

public class MockMethodImplementationGenerator<TArg1, TReturnType> : IMethodBodyGenerator<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> GenerateSwitchBody() =>
        new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBody<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params object[] cases)
        => new MockMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>();

    public IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
        => new MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1,
    TReturnType>
{
    public IMethodBodyGenerator<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> func)
        => new MockMethodImplementationGenerator<TArg1, TReturnType>();

    public IMethodBodyGenerator<TArg1, TReturnType> UseProvidedBody(Func<TArg1, Func<TReturnType>> func)
        => new MockMethodImplementationGenerator<TArg1, TReturnType>();
}

public class MockMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType> : IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory)
        => new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();

    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> UseBody(Func<TArg1, Action<TReturnType>> body)
        => new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
}

public class MockMethodBodyBuilder : IMethodBodyBuilder
{
    public IMethodBodyBuilder<TArg1> WithParameter<TArg1>() => new MockMethodBodyBuilder<TArg1>();

    public IMethodBodyGenerator<TReturnType> WithReturnType<TReturnType>() => new MockImplementationGenerator<TReturnType>();
}

public class MockImplementationGenerator<TReturnType> : IMethodBodyGenerator<TReturnType>
{
    public IMethodBodyGenerator BodyReturningConstantValue(Func<object> body) => this;
}

public class MockMethodBodyBuilder<TArg1Input> : IMethodBodyBuilder<TArg1Input>
{
    public IMethodBodyGenerator<TArg1Input, TReturnType> WithReturnType<TReturnType>()
        => new MockMethodImplementationGenerator<TArg1Input, TReturnType>();
}