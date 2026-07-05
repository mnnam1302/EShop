using System.Text.Json;

namespace EShop.Finance.Application.Services.IntegrationProvider.Http;

internal static class JsonTemplateData
{
    public static object? Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Convert(document.RootElement);
    }

    public static IReadOnlyDictionary<string, string?> FlattenObject(string json)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in document.RootElement.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Null => null,
                JsonValueKind.Object or JsonValueKind.Array => property.Value.GetRawText(),
                _ => property.Value.ToString(),
            };
        }

        return result;
    }

    private static object? Convert(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => Convert(p.Value)),
        JsonValueKind.Array => element.EnumerateArray().Select(Convert).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}
