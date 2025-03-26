namespace EShop.Shared.Cache.CacheKeys;

public class TenantFeaturesCacheKeyProvider
{
    private const string OwnerService = "tenancy";

    public static string GetCacheKey(string tenantId)
    {
        return string.Format("{0}:features:{1}", OwnerService, tenantId);
    }
}