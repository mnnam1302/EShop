namespace EShop.Shared.Cache.CacheKeys;

internal static class KeyEncryptionCacheKeyProvider
{
    private const string OwnerService = "authorization";

    public static string GetActiveKeyPairCacheKey(string tenantId)
        => $"{OwnerService}:rsa_keypair_active:{tenantId}";

    public static string GetPublicKeyCacheKey(string tenantId, string keyId)
        => $"{OwnerService}:rsa_publickey:{tenantId}:{keyId}";
}
