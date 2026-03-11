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
        IMethodBodyGenerator<int, string> implementation = result.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MethodBodyBodyBuilder<int>>());
        Assert.That(implementation, Is.TypeOf<TrackingArgImplementationGenerator<int, string>>());
        Assert.That(factory.ArgCreateImplementationCalls, Is.EqualTo(1));
    }

    [Test]
    public void WithReturnType_OnNonGenericBuilder_UsesFactoryCreateImplementation()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBodyBuilder bodyBuilder = new MethodBodyBuilder(factory);

        IMethodBodyGenerator<string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<TrackingTypedImplementationGenerator<string>>());
        Assert.That(factory.TypedCreateImplementationCalls, Is.EqualTo(1));
    }

    [Test]
    public void WithReturnType_OnGenericBuilder_UsesFactoryCreateImplementationWithArg()
    {
        TrackingGeneratorsFactory factory = new TrackingGeneratorsFactory();
        MethodBodyBodyBuilder<int> bodyBuilder = new MethodBodyBodyBuilder<int>(factory);

        IMethodBodyGenerator<int, string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<TrackingArgImplementationGenerator<int, string>>());
        Assert.That(factory.ArgCreateImplementationCalls, Is.EqualTo(1));
    }

    private sealed class TrackingGeneratorsFactory : IMethodBodyGeneratorStage0
    {
        public int TypedCreateImplementationCalls { get; private set; }
        public int ArgCreateImplementationCalls { get; private set; }

        public IMethodBodyGeneratorWithNoParameter CreateImplementation() => new TrackingNoParamGenerator();

        public IMethodBodyBuilder ForMethod() => new MethodBodyBuilder(this);

        public IMethodBodyGenerator<TReturnType> CreateImplementation<TReturnType>()
        {
            TypedCreateImplementationCalls++;
            return new TrackingTypedImplementationGenerator<TReturnType>();
        }

        public IMethodBodyGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>()
        {
            ArgCreateImplementationCalls++;
            return new TrackingArgImplementationGenerator<TArg1, TReturnType>();
        }
    }

    private sealed class TrackingNoParamGenerator : IMethodBodyGeneratorWithNoParameter;

    private sealed class TrackingTypedImplementationGenerator<TReturnType> : IMethodBodyGenerator<TReturnType>
    {
        public IMethodBodyGeneratorWithNoParameter BodyReturningConstantValue(Func<object> body) =>
            new TrackingNoParamGenerator();
    }

    private sealed class TrackingArgImplementationGenerator<TArg1, TReturnType> : IMethodBodyGenerator<TArg1, TReturnType>
    {
        public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> GenerateSwitchBody() =>
            new MockMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>();
    }
}
