using System.Reflection;
using EasySourceGenerators.Abstractions;
using EasySourceGenerators.Tests.Generation.Passing.Helpers;

namespace EasySourceGenerators.Tests.Generation.Passing;

public class ColorsTestsEntireMethod
{
    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedRuntimeOutput()
    {
        TestColorsClassEntireMethod testColorsClass = new TestColorsClassEntireMethod();

        MethodInfo? generatedMethod = typeof(TestColorsClassEntireMethod).GetMethod("GetAllColorsString");

        Assert.That(generatedMethod, Is.Not.Null, "Could not find the generated method");

        string? allColors = (string?) generatedMethod.Invoke(testColorsClass, []);

        Assert.That(allColors, Is.EqualTo("Red, Green, Blue"));
    }

    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestColorsClassEntireMethod_GetAllColorsString.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests.Generation.Passing;

                              partial class TestColorsClassEntireMethod
                              {
                                  public string GetAllColorsString()
                                  {
                                      return "Red, Green, Blue";
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class TestColorsClassEntireMethod
{
    [MethodGenerator]
    private static IMethodGenerator GetAllColorsString_Generator() =>
        Generate.Method("GetAllColorsString")
            .WithReturnType<string>().WithNoParameters()
            .BodyReturningConstant(() => string.Join(", ", Enum.GetNames<TestColorsEnum>()));
}
