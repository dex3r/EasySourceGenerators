using EasySourceGenerators.Abstractions;
// ReSharper disable ConvertClosureToMethodGroup

namespace EasySourceGenerators.Examples;

// NOTE: [SwitchCase] attribute-based generation is commented out pending replacement
// with a data-driven approach. See PiExampleFluent.cs for the fluent API equivalent.
// public static partial class PiExample
// {
//     public static partial int GetPiDecimal(int decimalNumber);
//
//     [GeneratesMethod(nameof(GetPiDecimal))]
//     [SwitchCase(arg1: 0)]
//     [SwitchCase(arg1: 1)]
//     [SwitchCase(arg1: 2)]
//     static int GetPiDecimal_Generator_Specialized(int decimalNumber) =>
//         SlowMath.CalculatePiDecimal(decimalNumber);
//     
//     [GeneratesMethod(nameof(GetPiDecimal))]
//     [SwitchDefault]
//     static Func<int, int> GetPiDecimal_Generator_Fallback() => decimalNumber => SlowMath.CalculatePiDecimal(decimalNumber);
// }

/*
 This will generate the following method:

    public static int GetPiDecimal(int decimalNumber)
    {
        switch (decimalNumber)
        {
            case 0: return 3;
            case 1: return 1;
            case 2: return 4;
            default: return CalculatePiDecimal(decimalNumber);
        }
    }
*/