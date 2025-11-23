namespace EShop.Shared.DomainTools.Extensions;

public static class StringExtensions
{
    public static string ToCommaSeparatedString(this IEnumerable<string> list, string? separator = null)
    {
        separator ??= ", ";
        return string.Join(separator, list.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}