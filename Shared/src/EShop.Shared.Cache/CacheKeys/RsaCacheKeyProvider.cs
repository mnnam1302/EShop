namespace EShop.Shared.Cache.CacheKeys;

internal static class RsaCacheKeyProvider
{
    private const string OwnerService = "authorization";

    /// <summary>
    /// Generates cache key for active RSA key pair for a specific tenant.
    /// Pattern: "authorization:rsa_keypair:{tenantId}"
    /// </summary>
    public static string GetRsaKeyPairCacheKey(string tenantId) => $"{OwnerService}:rsa_keypair:{tenantId}";

    /// <summary>
    /// Generates cache key for RSA public key for a specific tenant and key ID.
    /// Pattern: "authorization:rsa_public:{tenantId}:{keyId}"
    /// </summary>
    public static string GetPublicKeyCacheKey(string tenantId, string keyId) => $"{OwnerService}:rsa_publickey:{tenantId}:{keyId}";
}
