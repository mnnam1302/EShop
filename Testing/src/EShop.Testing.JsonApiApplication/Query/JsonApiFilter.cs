namespace EShop.Testing.JsonApiApplication.Query;

/// <summary>
/// Provides static factory methods for building JSON:API filter expressions
/// that conform to the JsonApiDotNetCore expression syntax.
/// </summary>
/// <remarks>
/// Compose expressions using <see cref="And"/>, <see cref="Or"/>, and <see cref="Not"/> combinators.
/// Pass the resulting expression to <see cref="JsonApiQueryBuilder.Filter"/>.
/// </remarks>
public static class JsonApiFilter
{
    /// <summary>Matches resources where <paramref name="field"/> equals <paramref name="value"/>.</summary>
    public static string Equals(string field, string value)
    {
        return $"equals({field},'{Escape(value)}')";
    }

    /// <summary>Negates the given filter <paramref name="expression"/>.</summary>
    public static string Not(string expression)
    {
        return $"not({expression})";
    }

    /// <summary>
    /// Combines multiple filter expressions with a logical AND.
    /// Returns the single expression unchanged when only one is supplied.
    /// </summary>
    public static string And(params string[] expressions)
    {
        return expressions.Length == 1
                ? expressions[0]
                : $"and({string.Join(',', expressions)})";
    }

    /// <summary>
    /// Combines multiple filter expressions with a logical OR.
    /// Returns the single expression unchanged when only one is supplied.
    /// </summary>
    public static string Or(params string[] expressions)
    {
        return expressions.Length == 1
                ? expressions[0]
                : $"or({string.Join(',', expressions)})";
    }

    /// <summary>Escapes single quotes inside a filter value.</summary>
    private static string Escape(string value)
    {
        return value.Replace("'", "\\'");
    }
}
