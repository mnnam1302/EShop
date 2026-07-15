namespace EShop.Shared.Cache.CacheKeys;

public static class RateLimitPolicyCacheKeyProvider
{
    private const string KeyPrefix = "tenancy:ratelimit-policy";

    public static string GetCacheKey(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        return $"{KeyPrefix}:{tenantId.Trim()}";
    }
}
