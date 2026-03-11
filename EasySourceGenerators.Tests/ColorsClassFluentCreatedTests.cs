using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

[TestFixture]
public class ColorsClassFluentCreatedTests
{
    [Test]
    public void ColorsClassFluentCreated_ProducesExpectedRuntimeOutput()
    {
        TestColorsClassFluentCreated testColorsClass = new TestColorsClassFluentCreated();

        string allColors = testColorsClass.GetAllColorsString();

        Assert.That(allColors, Is.EqualTo("Red, Green, Blue"));
    }

    [Test]
    public void ColorsClassFluentCreated_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestColorsClassFluentCreated_GetAllColorsString.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              partial class TestColorsClassFluentCreated
                              {
                                  public System.String GetAllColorsString()
                                  {
                                      return "Red, Green, Blue";
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class TestColorsClassFluentCreated
{
    [MethodBodyGenerator("GetAllColorsString")]
    static IMethodBodyGenerator GetAllColorsString_Generator() =>
        Generate.Method()
            .WithName("GetAllColorsString")
            .WithReturnType<string>()
            .WithNoParameters()
            .BodyReturningConstant(() => string.Join(", ", Enum.GetNames<TestColorsEnum>()));
}
