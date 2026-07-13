namespace EShop.Shared.RateLimiting;

public static class RateLimitKeyBuilder
{
    private const string AnonymousHashTag = "_";

    public static string TenantQuotaKey(string tenantId, string domain)
    {
        return "rl:{" + tenantId + "}:quota:" + domain;
    }

    public static string UserBucketKey(string tenantId, string userId, string domain)
    {
        return "rl:{" + tenantId + "}:u:" + userId + ":" + domain;
    }

    public static string AnonymousIpKey(string ipAddress, string domain)
    {
        return "rl:{" + AnonymousHashTag + "}:ip:" + ipAddress + ":" + domain;
    }
}
