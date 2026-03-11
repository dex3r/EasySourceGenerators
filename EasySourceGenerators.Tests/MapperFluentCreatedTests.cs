using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

[TestFixture]
public class MapperFluentCreatedTests
{
    [TestCase(TestFourLeggedAnimal.Dog, TestMammalAnimal.Dog)]
    [TestCase(TestFourLeggedAnimal.Cat, TestMammalAnimal.Cat)]
    public void MapperFluentCreated_ProducesExpectedRuntimeOutput(TestFourLeggedAnimal source, TestMammalAnimal expected)
    {
        TestMapperFluentCreated mapper = new TestMapperFluentCreated();

        TestMammalAnimal result = mapper.MapToMammal(source);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MapperFluentCreated_ThrowsForUnmappableValue()
    {
        TestMapperFluentCreated mapper = new TestMapperFluentCreated();

        Assert.Throws<ArgumentException>(() => mapper.MapToMammal(TestFourLeggedAnimal.Lizard));
    }

    [Test]
    public void MapperFluentCreated_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestMapperFluentCreated_MapToMammal.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              partial class TestMapperFluentCreated
                              {
                                  public EasySourceGenerators.Tests.TestMammalAnimal MapToMammal(EasySourceGenerators.Tests.TestFourLeggedAnimal fourLeggedAnimal)
                                  {
                                      switch (fourLeggedAnimal)
                                      {
                                          case EasySourceGenerators.Tests.TestFourLeggedAnimal.Dog: return EasySourceGenerators.Tests.TestMammalAnimal.Dog;
                                          case EasySourceGenerators.Tests.TestFourLeggedAnimal.Cat: return EasySourceGenerators.Tests.TestMammalAnimal.Cat;
                                          default: throw new ArgumentException($"Cannot map {fourLeggedAnimal} to a Mammal");
                                      }
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class TestMapperFluentCreated
{
    [MethodBodyGenerator("MapToMammal")]
    static IMethodBodyGenerator MapToMammal_Generator() =>
        Generate.Method()
            .WithName("MapToMammal")
            .WithReturnType<TestMammalAnimal>()
            .WithParameter<TestFourLeggedAnimal>()
            .BodyWithSwitchStatement()
            .ForCases(GetFourLeggedAnimalsThatAreAlsoMammal()).ReturnConstantValue(a => Enum.Parse<TestMammalAnimal>(a.ToString(), true))
            .ForDefaultCase().UseProvidedBody(fourLeggedAnimal => throw new ArgumentException($"Cannot map {fourLeggedAnimal} to a Mammal"));

    static TestFourLeggedAnimal[] GetFourLeggedAnimalsThatAreAlsoMammal() =>
        Enum.GetValues<TestFourLeggedAnimal>()
            .Where(a => Enum.TryParse(typeof(TestMammalAnimal), a.ToString(), true, out _))
            .ToArray();
}
