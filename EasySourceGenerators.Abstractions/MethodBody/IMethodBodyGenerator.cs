namespace EasySourceGenerators.Abstractions;

/*
 IMethodBodyGenerator implements IMethodGenerator, because method generating entire method will return IMethodBodyGenerator in the end.
 
 [MethodGenerator]
 IMethodGenerator SomeGenerator() =>
     Generate.Method("GetAllColorsString")
         .WithReturnType<string>().WithNoParameters()
         .BodyReturningConstant(() => "placeholder"));
*/

public interface IMethodBodyGenerator : IMethodGenerator;

public interface IMethodGenerator;