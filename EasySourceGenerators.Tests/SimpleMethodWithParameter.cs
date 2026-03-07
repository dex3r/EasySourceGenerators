using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

public class SimpleMethodWithParameterTests
{
    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedRuntimeOutput()
    {
        SimpleMethodWithParameterClass testColorsClass = new SimpleMethodWithParameterClass();

        int result = testColorsClass.SimpleMethodWithParameter(123123);

        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void ColorsClassLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("SimpleMethodWithParameterClass_SimpleMethodWithParameter.g.cs");
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              partial class SimpleMethodWithParameterClass
                              {
                                  public partial int SimpleMethodWithParameter(int someIntParameter)
                                  {
                                      return 5;
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public partial class SimpleMethodWithParameterClass
{
    public partial int SimpleMethodWithParameter(int someIntParameter);

    [GeneratesMethod(sameClassMethodName: nameof(SimpleMethodWithParameter))]
    private static int SimpleMethodWithParameter_Generator(int someIntParameter)
    {
        return 5;
    }
}
