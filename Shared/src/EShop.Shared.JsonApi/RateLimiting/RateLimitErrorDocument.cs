namespace EShop.Shared.JsonApi.RateLimiting;

internal sealed class RateLimitErrorDocument
{
    public required IReadOnlyList<RateLimitErrorObject> Errors { get; init; }
}

internal sealed class RateLimitErrorObject
{
    public required string Status { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required string Detail { get; init; }
}
