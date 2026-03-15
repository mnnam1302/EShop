using Newtonsoft.Json;

namespace EShop.Testing.JsonApiApplication.Query;

/// <summary>
/// Top-level JSON:API document returned by collection endpoints (GET /resources).
/// </summary>
public sealed class JsonApiCollectionDocument<TAttributes>
{
    [JsonProperty("data")]
    public List<JsonApiResource<TAttributes>> Data { get; init; } = [];

    [JsonProperty("meta")]
    public JsonApiMeta? Meta { get; init; }
}

/// <summary>
/// Top-level JSON:API document returned by single-resource endpoints (GET /resources/{id}).
/// </summary>
public sealed class JsonApiSingleDocument<TAttributes>
{
    [JsonProperty("data")]
    public JsonApiResource<TAttributes>? Data { get; init; }
}

/// <summary>
/// A single resource object as defined by the JSON:API specification.
/// </summary>
public sealed class JsonApiResource<TAttributes>
{
    /// <summary>The JSON:API resource <c>id</c> (e.g. the MongoDB hex string for MongoDb-backed resources).</summary>
    [JsonProperty("id")]
    public string? Id { get; init; }

    /// <summary>The JSON:API resource <c>type</c> (e.g. <c>"categories"</c>).</summary>
    [JsonProperty("type")]
    public string? Type { get; init; }

    /// <summary>The resource attributes, deserialized into <typeparamref name="TAttributes"/>.</summary>
    [JsonProperty("attributes")]
    public TAttributes? Attributes { get; init; }
}

/// <summary>
/// JSON:API top-level <c>meta</c> object.
/// </summary>
public sealed class JsonApiMeta
{
    [JsonProperty("total")]
    public int? Total { get; init; }
}
