using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Generators;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class MethodBodyBodyBuilderTests
{
    [Test]
    public void WithParameter_ReturnsGenericMethodBuilder()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBodyBuilder bodyBuilder = new MethodBodyBuilder(factory);

        IMethodBodyBuilder<int> result = bodyBuilder.WithParameter<int>();
        IMethodImplementationGenerator<int, string> implementation = result.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MethodBodyBodyBuilder<int>>());
        Assert.That(implementation, Is.TypeOf<TrackingArgImplementationGenerator<int, string>>());
        Assert.That(factory.ArgCreateImplementationCalls, Is.EqualTo(1));
    }

    [Test]
    public void WithReturnType_OnNonGenericBuilder_UsesFactoryCreateImplementation()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBodyBuilder bodyBuilder = new MethodBodyBuilder(factory);

        IMethodImplementationGenerator<string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<TrackingTypedImplementationGenerator<string>>());
        Assert.That(factory.TypedCreateImplementationCalls, Is.EqualTo(1));
    }

    [Test]
    public void WithReturnType_OnGenericBuilder_UsesFactoryCreateImplementationWithArg()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBodyBodyBuilder<int> bodyBuilder = new MethodBodyBodyBuilder<int>(factory);

        IMethodImplementationGenerator<int, string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<TrackingArgImplementationGenerator<int, string>>());
        Assert.That(factory.ArgCreateImplementationCalls, Is.EqualTo(1));
    }

    private sealed class TrackingGeneratorsFactory : IGeneratorsFactory
    {
        public int TypedCreateImplementationCalls { get; private set; }
        public int ArgCreateImplementationCalls { get; private set; }

        public IMethodBodyBuilder ForMethod() => new MethodBodyBuilder(this);

        public IMethodImplementationGenerator<TReturnType> CreateImplementation<TReturnType>()
        {
            TypedCreateImplementationCalls++;
            return new TrackingTypedImplementationGenerator<TReturnType>();
        }

        public IMethodImplementationGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>()
        {
            ArgCreateImplementationCalls++;
            return new TrackingArgImplementationGenerator<TArg1, TReturnType>();
        }
    }

    private sealed class TrackingTypedImplementationGenerator<TReturnType> : IMethodImplementationGenerator<TReturnType>
    {
        public IMethodImplementationGenerator UseBody(Func<object> body) => this;
    }

    private sealed class TrackingArgImplementationGenerator<TArg1, TReturnType> : IMethodImplementationGenerator<TArg1, TReturnType>
    {
        public IMethodImplementationGeneratorSwitchBody<TArg1, TReturnType> WithSwitchBody() =>
            new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
    }
}
