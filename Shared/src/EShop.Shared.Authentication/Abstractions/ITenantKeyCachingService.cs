namespace EShop.Shared.Authentication.Abstractions;

public interface ITenantKeyCachingService
{
    Task<RsaKeyPair?> GetAsync(string tenantId, CancellationToken cancellationToken);

    Task AddAsync(string tenantId, RsaKeyPair keyPair, CancellationToken cancellationToken);

    Task RemoveAsync(string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the active RSA key pair for a tenant.
    /// </summary>
    Task<RsaKeyPair?> GetActiveKeyAsync(string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the previous RSA key pair for a tenant (used during key rotation).
    /// </summary>
    Task<RsaKeyPair?> GetPreviousKeyAsync(string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the active RSA key pair for a tenant.
    /// </summary>
    Task SetActiveKeyAsync(string tenantId, RsaKeyPair keyPair, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the previous RSA key pair for a tenant with TTL.
    /// </summary>
    Task SetPreviousKeyAsync(string tenantId, RsaKeyPair keyPair, TimeSpan ttl, CancellationToken cancellationToken);
}
