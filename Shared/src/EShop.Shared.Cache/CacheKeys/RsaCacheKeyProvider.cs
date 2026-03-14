namespace EShop.Shared.Cache.CacheKeys;

internal static class RsaCacheKeyProvider
{
    private const string OwnerService = "authorization";

    /// <summary>
    /// Generates cache key for active RSA key pair for a specific tenant.
    /// Pattern: "authorization:keys:{tenantId}:active"
    /// </summary>
    public static string GetActiveKeyCacheKey(string tenantId) => $"{OwnerService}:keys:{tenantId}:active";

    /// <summary>
    /// Generates cache key for previous RSA key pair for a specific tenant (used during key rotation).
    /// Pattern: "authorization:keys:{tenantId}:previous"
    /// </summary>
    public static string GetPreviousKeyCacheKey(string tenantId) => $"{OwnerService}:keys:{tenantId}:previous";

    /// <summary>
    /// Legacy method for backward compatibility. Use GetActiveKeyCacheKey instead.
    /// Pattern: "authorization:keypairs:{tenantId}"
    /// </summary>
    [Obsolete("Use GetActiveKeyCacheKey instead")]
    public static string GetRsaKeyPairCacheKey(string tenantId) => $"{OwnerService}:keypairs:{tenantId}";
}
