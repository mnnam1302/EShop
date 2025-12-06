using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EShop.Shared.DomainTools.Extensions;

public static class NullCheckExtensions
{
    /// <summary>
    /// Asserts that the specified object is not null.
    /// </summary>
    /// <param name="o">The object to be checked for nullability.</param>
    /// <param name="errorMessage">An optional custom error message of the <see cref="InvalidOperationException"/>
    /// that is thrown if the object is null.</param>
    /// <param name="expression"></param>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <returns>The object that is guaranteed to be non-nullable.</returns>
    /// <example>The following code
    /// <code>
    /// public void Method(Person person)
    /// {
    ///     var capitalizedName = person.Name.Require().ToUpper();
    /// }
    /// </code>
    /// will throw <see cref="InvalidOperationException"/> with message "person.Name should not be null at this point." if Name is null.
    /// </example>
    /// <exception cref="InvalidOperationException">Thrown if the specified object is null.</exception>
    public static T Require<T>([NotNull] this T? o, string? errorMessage = null, [CallerArgumentExpression("o")] string? expression = null)
        where T : class
    {
        if (o is not null)
            return o;

        if (errorMessage is not { Length: > 0 })
        {
            errorMessage = expression is { Length: > 0 }
                ? $"{expression} should not be null at this point."
                : "Expected a non-null value.";
        }

        throw new InvalidOperationException(errorMessage);
    }
}
