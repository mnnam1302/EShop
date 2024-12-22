using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EShop.Shared.Scoping.DependencyInjections;

/// <summary>
/// Provides extension methods that assert that an object or an argument is not null.
/// </summary>
public static class NullCheckExtensions
{
    /// <summary>
    /// Asserts that the specified nullable struct is not null.
    /// </summary>
    /// <param name="s">The struct to be checked for nullability.</param>
    /// <param name="errorMessage">An optional custom error message of the <see cref="InvalidOperationException"/>
    /// that is thrown if the struct is null.</param>
    /// <param name="expression"></param>
    /// <typeparam name="T">Type of the struct.</typeparam>
    /// <returns>The wrapped value of the nullable struct.</returns>
    /// <example>The following code
    /// <code>
    /// public void Method(Person person)
    /// {
    ///     var dateOfBirth = person.DateOfBirth.Require();
    /// }
    /// </code>
    /// will throw <see cref="InvalidOperationException"/> with message "person.DateOfBirth should not be null at this point." if DateOfBirth is null.
    /// </example>
    /// <exception cref="InvalidOperationException">Thrown if the specified nullable struct is null.</exception>
    public static T Require<T>([NotNull] this T? s, string? errorMessage = null, [CallerArgumentExpression("s")] string? expression = null)
        where T : struct
    {
        if (s is not null)
            return s.Value;

        if (errorMessage is not { Length: > 0 })
        {
            errorMessage = expression is { Length: > 0 }
                ? $"{expression} should not be null at this point."
                : "Expected a non-null value.";
        }

        throw new InvalidOperationException(errorMessage);
    }
}