namespace EasySourceGenerators.Abstractions;

public interface IGeneratorsFactory
{
    IMethodBodyBuilderStage1 StartFluentApiBuilderForBody();
}