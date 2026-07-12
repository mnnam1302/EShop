namespace EShop.Shared.Cache.Services;

public sealed class CachedRateLimitPolicy
{
    public required bool HasPolicy { get; init; }
    public required IReadOnlyList<CachedRateLimitRule> Rules { get; init; }
}

public sealed class CachedRateLimitRule
{
    public required string Domain { get; init; }
    public required string Scope { get; init; }
    public required string Unit { get; init; }
    public required int RequestsPerUnit { get; init; }
    public int? Burst { get; init; }
}
