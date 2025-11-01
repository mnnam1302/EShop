namespace EShop.Shared.Authentication.Abstractions;

public interface ITenantKeyProvider
{
    Task<RsaKeyPair> GetOrCreateKeyPairAsync(string tenantId, CancellationToken cancellationToken);

    Task<RsaKeyPair> GetKeyPairAsync(string tenantId, CancellationToken cancellationToken);

    Task RotateKeyPairAsync(string tenantId, CancellationToken cancellationToken);
}
