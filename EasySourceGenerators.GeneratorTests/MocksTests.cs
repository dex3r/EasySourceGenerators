using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.GeneratorTests;

[TestFixture]
public class MocksTests
{
    [Test]
    public void MockGeneratorsFactory_ForMethod_ReturnsMockMethodBuilder()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBodyBuilder result = factory.ForMethod();

        Assert.That(result, Is.TypeOf<MockMethodBodyBuilder>());
    }

    [Test]
    public void MockGeneratorsFactory_CreateImplementationTyped_ReturnsTypedMock()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBodyGenerator<string> result = factory.CreateImplementation<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<string>>());
    }

    [Test]
    public void MockGeneratorsFactory_CreateImplementationWithArg_ReturnsArgMock()
    {
        MockGeneratorsFactory factory = new MockGeneratorsFactory();

        IMethodBodyGenerator<int, string> result = factory.CreateImplementation<int, string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }

    [Test]
    public void MockMethodImplementationGeneratorTyped_UseBody_ReturnsWithNoParameter()
    {
        MockMethodImplementationGenerator<string> generator = new MockMethodImplementationGenerator<string>();

        IMethodBodyGeneratorWithNoParameter result = generator.BodyReturningConstantValue(() => "x");

        Assert.That(result, Is.TypeOf<MockMethodBodyGeneratorWithNoParameter>());
    }

    [Test]
    public void MockMethodImplementationGeneratorWithArg_WithSwitchBody_ReturnsSwitchBodyMock()
    {
        MockMethodImplementationGenerator<int, string> generator = new MockMethodImplementationGenerator<int, string>();

        IMethodBodyGeneratorSwitchBody<int, string> result = generator.GenerateSwitchBody();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockSwitchBody_ForCases_ReturnsCaseMock()
    {
        MockMethodImplementationGeneratorSwitchBody<int, string> switchBody = new MockMethodImplementationGeneratorSwitchBody<int, string>();

        IMethodBodyGeneratorSwitchBodyCase<int, string> result = switchBody.ForCases(1, 2);

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBodyCase<int, string>>());
    }

    [Test]
    public void MockSwitchBody_ForDefaultCase_ReturnsDefaultCaseMock()
    {
        MockMethodImplementationGeneratorSwitchBody<int, string> switchBody = new MockMethodImplementationGeneratorSwitchBody<int, string>();

        IMethodBodyGeneratorSwitchBodyDefaultCase<int, string> result = switchBody.ForDefaultCase();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>>());
    }

    [Test]
    public void MockDefaultCase_ReturnConstantValue_ReturnsMethodBodyGenerator()
    {
        MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> defaultCase = new MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>();

        IMethodBodyGenerator result = defaultCase.ReturnConstantValue(value => value.ToString());

        Assert.That(result, Is.TypeOf<MockMethodBodyGenerator>());
    }

    [Test]
    public void MockDefaultCase_UseProvidedBody_ReturnsMethodBodyGenerator()
    {
        MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string> defaultCase = new MockMethodImplementationGeneratorSwitchBodyDefaultCase<int, string>();

        IMethodBodyGenerator result = defaultCase.UseProvidedBody(_ => "v");

        Assert.That(result, Is.TypeOf<MockMethodBodyGenerator>());
    }

    [Test]
    public void MockCase_ReturnConstantValue_ReturnsSwitchBody()
    {
        MockMethodImplementationGeneratorSwitchBodyCase<int, string> caseBuilder = new MockMethodImplementationGeneratorSwitchBodyCase<int, string>();

        IMethodBodyGeneratorSwitchBody<int, string> result = caseBuilder.ReturnConstantValue(value => value.ToString());

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockCase_UseProvidedBody_ReturnsSwitchBody()
    {
        MockMethodImplementationGeneratorSwitchBodyCase<int, string> caseBuilder = new MockMethodImplementationGeneratorSwitchBodyCase<int, string>();

        IMethodBodyGeneratorSwitchBody<int, string> result = caseBuilder.UseProvidedBody(_ => "test");

        Assert.That(result, Is.TypeOf<MockMethodImplementationGeneratorSwitchBody<int, string>>());
    }

    [Test]
    public void MockMethodBuilder_WithParameter_ReturnsGenericBuilder()
    {
        MockMethodBodyBuilder bodyBuilder = new MockMethodBodyBuilder();

        IMethodBodyBuilder<int> result = bodyBuilder.WithParameter<int>();

        Assert.That(result, Is.TypeOf<MockMethodBodyBuilder<int>>());
    }

    [Test]
    public void MockMethodBuilder_WithReturnType_ReturnsMockImplementationGenerator()
    {
        MockMethodBodyBuilder bodyBuilder = new MockMethodBodyBuilder();

        IMethodBodyGenerator<string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<string>>());
    }

    [Test]
    public void MockImplementationGenerator_UseBody_ReturnsWithNoParameter()
    {
        MockMethodImplementationGenerator<string> generator = new MockMethodImplementationGenerator<string>();

        IMethodBodyGeneratorWithNoParameter result = generator.BodyReturningConstantValue(() => "x");

        Assert.That(result, Is.TypeOf<MockMethodBodyGeneratorWithNoParameter>());
    }

    [Test]
    public void MockMethodBuilderGeneric_WithReturnType_ReturnsArgImplementationGenerator()
    {
        MockMethodBodyBuilder<int> bodyBuilder = new MockMethodBodyBuilder<int>();

        IMethodBodyGenerator<int, string> result = bodyBuilder.WithReturnType<string>();

        Assert.That(result, Is.TypeOf<MockMethodImplementationGenerator<int, string>>());
    }
}
