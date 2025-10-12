namespace EShop.Shared.Cache.CacheKeys;

internal static class RsaKeyCacheKeyProvider
{
    private const string OwnerService = "authorization";

    /// <summary>
    /// Generates cache key for active RSA key pair for a specific tenant.
    /// Pattern: "authorization:rsa:active:{tenantId}"
    /// </summary>
    public static string GetActiveKeyPairCacheKey(string tenantId)
        => $"{OwnerService}:rsa_keypair_active:{tenantId}";

    /// <summary>
    /// Generates cache key for RSA public key for a specific tenant and key ID.
    /// Pattern: "authorization:rsa:public:{tenantId}:{keyId}"
    /// </summary>
    public static string GetPublicKeyCacheKey(string tenantId, string keyId)
        => $"{OwnerService}:rsa_publickey:{tenantId}:{keyId}";
}
