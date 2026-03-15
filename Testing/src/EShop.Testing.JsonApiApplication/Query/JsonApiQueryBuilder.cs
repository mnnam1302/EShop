using System.Text;

namespace EShop.Testing.JsonApiApplication.Query;

/// <summary>
/// Fluent builder for JSON:API compliant query strings.ies
/// Supports filtering, sorting, pagination, sparse fieldsets, and relationship includes
/// as defined in the JSON:API specification (https://jsonapi.org/format/#fetching).
/// </summary>
/// <remarks>
/// Use <see cref="JsonApiFilter"/> to construct filter expressions and pass them to <see cref="Filter"/>.
/// <code>
/// var url = JsonApiQueryBuilder.New()
///     .Filter(JsonApiFilter.Equals("reference", "ELEC123"))
///     .PageSize(10)
///     .SortAscending("name")
///     .Fields("categories", "name", "reference", "slug")
///     .ApplyTo("/api/v1/categories");
/// </code>
/// </remarks>
public sealed class JsonApiQueryBuilder
{
    private readonly List<string> _filters = [];
    private readonly List<string> _sorts = [];
    private readonly Dictionary<string, List<string>> _sparseFields = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _includes = [];
    private int? _pageNumber;
    private int? _pageSize;

    public static JsonApiQueryBuilder New() => new();

    /// <summary>
    /// Adds a filter expression. Multiple calls are combined with a logical AND.
    /// Use <see cref="JsonApiFilter"/> to build expression strings.
    /// </summary>
    public JsonApiQueryBuilder Filter(string filterExpression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterExpression);
        _filters.Add(filterExpression);
        return this;
    }

    public JsonApiQueryBuilder SortAscending(string field)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        _sorts.Add(field);
        return this;
    }

    public JsonApiQueryBuilder SortDescending(string field)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        _sorts.Add($"-{field}");
        return this;
    }

    public JsonApiQueryBuilder PageNumber(int number)
    {
        _pageNumber = number;
        return this;
    }

    public JsonApiQueryBuilder PageSize(int size)
    {
        _pageSize = size;
        return this;
    }

    public JsonApiQueryBuilder Fields(string resourceType, params string[] fieldNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        if (!_sparseFields.TryGetValue(resourceType, out var list))
        {
            list = [];
            _sparseFields[resourceType] = list;
        }

        list.AddRange(fieldNames);
        return this;
    }

    public JsonApiQueryBuilder Include(params string[] relationships)
    {
        _includes.AddRange(relationships);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        AppendFilter(sb);
        AppendSort(sb);
        AppendPage(sb);
        AppendFields(sb);
        AppendIncludes(sb);

        return sb.ToString();
    }

    /// <summary>
    /// Returns <paramref name="baseUrl"/> with the built query string appended.
    /// Uses <c>?</c> or <c>&amp;</c> as the separator depending on whether
    /// <paramref name="baseUrl"/> already contains a query string.
    /// </summary>
    public string ApplyTo(string baseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);

        var queryString = Build();
        if (string.IsNullOrEmpty(queryString))
        {
            return baseUrl;
        }

        var separator = baseUrl.Contains('?') ? '&' : '?';
        return $"{baseUrl}{separator}{queryString}";
    }

    private void AppendFilter(StringBuilder sb)
    {
        if (_filters.Count == 0)
        {
            return;
        }

        var expression = _filters.Count == 1
            ? _filters[0]
            : JsonApiFilter.And([.. _filters]);

        Append(sb, $"filter={expression}");
    }

    private void AppendSort(StringBuilder sb)
    {
        if (_sorts.Count > 0)
        {
            Append(sb, $"sort={string.Join(',', _sorts)}");
        }
    }

    private void AppendPage(StringBuilder sb)
    {
        if (_pageNumber.HasValue)
        {
            Append(sb, $"page[number]={_pageNumber.Value}");
        }

        if (_pageSize.HasValue)
        {
            Append(sb, $"page[size]={_pageSize.Value}");
        }
    }

    private void AppendFields(StringBuilder sb)
    {
        foreach (var (resourceType, fields) in _sparseFields)
        {
            Append(sb, $"fields[{resourceType}]={string.Join(',', fields)}");
        }
    }

    private void AppendIncludes(StringBuilder sb)
    {
        if (_includes.Count > 0)
        {
            Append(sb, $"include={string.Join(',', _includes)}");
        }
    }

    private static void Append(StringBuilder sb, string part)
    {
        if (sb.Length > 0)
        {
            sb.Append('&');
        }

        sb.Append(part);
    }
}
