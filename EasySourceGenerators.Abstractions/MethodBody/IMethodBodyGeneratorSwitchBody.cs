namespace EasySourceGenerators.Abstractions;

public interface IMethodBodyGeneratorSwitchBody<TParam1, in TReturnType>
{
    IMethodBodyGeneratorSwitchBodyCase<TParam1, TReturnType> ForCases(params TParam1[] cases);
    IMethodBodyGeneratorSwitchBodyDefaultCase<TParam1, TReturnType> ForDefaultCase();
}

public interface IMethodBodyGeneratorSwitchBodyCase<out TParam1, in TReturnType>
{
    /// <summary>
    /// Specific case(s) will use the body provided.
    /// </summary>
    /// <param name="body">During code generation this body will be emitted.</param>
    IMethodBodyGeneratorSwitchBodyCaseStage2<TParam1, TReturnType> UseProvidedBody(Func<TParam1, TReturnType> body);
    
    /// <summary>
    /// Specify case(s) will return a constant value.
    /// </summary>
    /// <param name="constantValueFactory">During code generation, this delegate will be run to calculate the constant value.
    /// The delegate will not be used in the generated code. Only the value it produces after it's executed during code generation.</param>
    IMethodBodyGeneratorSwitchBodyCaseStage2<TParam1, TReturnType> ReturnConstantValue(Func<TParam1, TReturnType> constantValueFactory);
}

public interface IMethodBodyGeneratorSwitchBodyCaseStage2<out TParam1, in TReturnType>
{
    IMethodBodyGeneratorSwitchBodyDefaultCase<TParam1, TReturnType> ForDefaultCase();
}

public interface IMethodBodyGeneratorSwitchBodyDefaultCase<out TParam1, in TReturnType> : IMethodBodyGenerator
{
    IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body);
    IMethodBodyGenerator ReturnConstantValue(Func<TParam1, TReturnType> constantValueFactory);
}
