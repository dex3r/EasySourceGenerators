using System;
using System.Collections.Generic;
using System.Linq;
using EasySourceGenerators.Abstractions;

namespace EasySourceGenerators.Generators;

public class SwitchBodyRecord
{
    public List<object> CaseKeys { get; } = new();
    public List<object?> CaseValues { get; } = new();
    public bool HasDefaultCase { get; set; }
}

public class RecordingGeneratorsFactory : IMethodBodyGeneratorStage0
{
    public SwitchBodyRecord? LastRecord { get; private set; }

    public IMethodBodyGeneratorWithNoParameter CreateImplementation()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGenerator();
    }

    public IMethodBodyBuilder ForMethod() => new MethodBodyBuilder(this);

    public IMethodBodyGenerator<TReturnType> CreateImplementation<TReturnType>()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGeneratorTyped<TReturnType>();
    }

    public IMethodBodyGenerator<TArg1, TReturnType> CreateImplementation<TArg1, TReturnType>()
    {
        SwitchBodyRecord record = new SwitchBodyRecord();
        LastRecord = record;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGenerator : IMethodBodyGeneratorWithNoParameter;

public class RecordingMethodImplementationGeneratorTyped<TReturnType> : IMethodBodyGenerator<TReturnType>
{
    public IMethodBodyGeneratorWithNoParameter BodyReturningConstantValue(Func<object> body) => this;
}

public class RecordingMethodImplementationGenerator<TArg1, TReturnType>(SwitchBodyRecord record) : IMethodBodyGenerator<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> GenerateSwitchBody()
    {
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(SwitchBodyRecord record)
    : IMethodBodyGeneratorSwitchBody<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType> ForCases(params object[] cases)
    {
        List<TArg1> flatCases = FlattenCases(cases).ToList();
        return new RecordingMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>(record, flatCases);
    }

    public IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType> ForDefaultCase()
    {
        return new RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>(record);
    }

    private static IEnumerable<TArg1> FlattenCases(object[] cases)
    {
        foreach (object oneCase in cases)
        {
            switch (oneCase)
            {
                case TArg1[] arr:
                {
                    foreach (TArg1 item in arr)
                        yield return item;
                    break;
                }
                case TArg1 val:
                    yield return val;
                    break;
                default:
                    yield return (TArg1)Convert.ChangeType(oneCase, typeof(TArg1));
                    break;
            }
        }
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyCase<TArg1, TReturnType>(SwitchBodyRecord record, List<TArg1> cases)
    : IMethodBodyGeneratorSwitchBodyCase<TArg1, TReturnType>
{
    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> constantValueFactory)
    {
        foreach (TArg1? caseValue in cases)
        {
            TReturnType result = constantValueFactory(caseValue);
            record.CaseKeys.Add((object?)caseValue ?? throw new InvalidOperationException("Switch case value cannot be null"));
            record.CaseValues.Add(result);
        }
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(record);
    }

    public IMethodBodyGeneratorSwitchBody<TArg1, TReturnType> UseBody(Func<TArg1, Action<TReturnType>> body)
    {
        foreach (TArg1? caseValue in cases)
        {
            record.CaseKeys.Add((object?)caseValue ?? throw new InvalidOperationException("Switch case value cannot be null"));
            record.CaseValues.Add(null);
        }
        return new RecordingMethodImplementationGeneratorSwitchBody<TArg1, TReturnType>(record);
    }
}

public class RecordingMethodImplementationGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>(SwitchBodyRecord record)
    : IMethodBodyGeneratorSwitchBodyDefaultCase<TArg1, TReturnType>
{
    public IMethodBodyGenerator<TArg1, TReturnType> ReturnConstantValue(Func<TArg1, TReturnType> func)
    {
        record.HasDefaultCase = true;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }

    public IMethodBodyGenerator<TArg1, TReturnType> UseProvidedBody(Func<TArg1, Func<TReturnType>> func)
    {
        record.HasDefaultCase = true;
        return new RecordingMethodImplementationGenerator<TArg1, TReturnType>(record);
    }
}
