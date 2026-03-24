using JetBrains.Annotations;

namespace EasySourceGenerators.Abstractions;

//TODO: Probably should be removed, since we are not going to go with the "Fluent body" approach.
public static class Compiler
{
    /// <summary>
    /// Runs <paramref name="compileTimeConstantFactory"/> at compile time and replaces entire invocation of this method with the result.
    /// </summary>
    /// <example>
    /// The following code:
    /// <code>int a = <see cref="Compiler"/>.<see cref="CalculateConstant"/>(() => Math.Pi * 2);</code>
    /// will be replaced during compilation with the following code:
    /// <code>int a = 6.2831853071795862</code>
    /// </example>
    public static T CalculateConstant<T>([UsedImplicitly] Func<T> compileTimeConstantFactory)
    {
        throw new InvalidOperationException("This method is intended to be used only at compile time and cannot not be called at runtime.");
    }
}