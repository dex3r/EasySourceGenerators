using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Tests;

[TestFixture]
public class PiExampleTests
{
    [Test]
    public void PiExampleLikeGenerator_ProducesExpectedGeneratedCode()
    {
        string generatedCode = GeneratedCodeTestHelper.ReadGeneratedCode("TestPiExampleFluent_GetPiDecimal.g.cs");
        
        //TODO: The specifics here, like the formatting or where the "constants" is declared might be off here, but the general idea must be working
        string expectedCode = """
                              namespace EasySourceGenerators.Tests;

                              static partial class TestPiExampleFluent
                              {
                                  public static partial int GetPiDecimal(int decimalNumber)
                                  {
                                      var constants = new
                                      {
                                          PrecomputedTargets = (new int[] { 0, 1, 2, 300, 301, 302, 303 }).ToDictionary(i => i, i => SlowMath.CalculatePiDecimal(i))
                                      };
                                      
                                      if (constants.PrecomputedTargets.TryGetValue(decimalNumber, out int precomputedResult)) return precomputedResult;
                                      
                                      return SlowMath.CalculatePiDecimal(decimalNumber);
                                  }
                              }
                              """.ReplaceLineEndings("\n").TrimEnd();

        Assert.That(generatedCode, Is.EqualTo(expectedCode));
    }
}

public static partial class TestPiExampleFluent
{
    public static partial int GetPiDecimal(int decimalNumber);

    [MethodBodyGenerator(nameof(GetPiDecimal))]
    static IMethodBodyGenerator GetPiDecimal_Generator() =>
        Generate.MethodBody()
            .ForMethod().WithReturnType<int>().WithParameter<int>()
            .WithCompileTimeConstants(() => new
            {
                PrecomputedTargets = (new int[] { 0, 1, 2, 300, 301, 302, 303 }).ToDictionary(i => i, i => SlowMath.CalculatePiDecimal(i))
            })
            .UseProvidedBody((constants, decimalNumber) =>
            {
                if (constants.PrecomputedTargets.TryGetValue(decimalNumber, out int precomputedResult)) return precomputedResult;
                
                return SlowMath.CalculatePiDecimal(decimalNumber);
            });
}

