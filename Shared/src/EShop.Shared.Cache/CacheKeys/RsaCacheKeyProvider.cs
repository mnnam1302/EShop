namespace EShop.Shared.Cache.CacheKeys;

internal static class RsaCacheKeyProvider
{
    private const string OwnerService = "authorization";

    /// <summary>
    /// Generates cache key for active RSA key pair for a specific tenant.
    /// Pattern: "authorization:keypairs:{tenantId}"
    /// </summary>
    public static string GetRsaKeyPairCacheKey(string tenantId) => $"{OwnerService}:keypairs:{tenantId}";
}
