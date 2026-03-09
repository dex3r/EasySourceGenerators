namespace EasySourceGenerators.Abstractions;

public interface IMethodBodyBuilderStage1
{
    IMethodBodyBuilderStage2 ForMethod();
}

public interface IMethodBodyBuilderStage2
{
    IMethodBodyBuilderStage3ReturnVoid WithVoidReturnType();
    IMethodBodyBuilderStage3<TReturnType> WithReturnType<TReturnType>();
}

public interface IMethodBodyBuilderStage3ReturnVoid
{
    IMethodBodyBuilderStage4ReturnVoidNoArg WithNoParameters();
    IMethodBodyBuilderStage4ReturnVoid<TParam1> WithOneParameter<TParam1>();
}

public interface IMethodBodyBuilderStage3<TReturnType>
{
    IMethodBodyBuilderStage4NoArg<TReturnType> WithNoParameters();
    IMethodBodyBuilderStage4<TReturnType, TParam1> WithOneParameter<TParam1>();
}

public interface IMethodBodyBuilderStage4ReturnVoidNoArg
{
    IMethodBodyGenerator UseProvidedBody(Action body);
}

public interface IMethodBodyBuilderStage4NoArg<in TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<TReturnType> body);
    IMethodBodyGenerator GenerateBodyRetuningConstant(Func<TReturnType> constantValueFactory);
}

public interface IMethodBodyBuilderStage4ReturnVoid<out TParam1>
{
    IMethodBodyGenerator UseProvidedBody(Action<TParam1> body);
}

public interface IMethodBodyBuilderStage4<out TParam1, in TReturnType>
{
    IMethodBodyGenerator UseProvidedBody(Func<TParam1, TReturnType> body);
    IMethodBodyGenerator GenerateBodyRetuningConstant(Func<TReturnType> constantValueFactory);
    IMethodBodyGeneratorSwitchBody<TParam1, TReturnType> GenerateBodyWithSwitchStatement();
}