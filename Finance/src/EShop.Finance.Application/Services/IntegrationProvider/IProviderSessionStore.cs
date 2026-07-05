using EShop.Finance.Application.Services.IntegrationProvider.Models;

namespace EShop.Finance.Application.Services.IntegrationProvider;

/// <summary>
/// Per-tenant OAuth token cache backed by the Finance database, with the token encrypted at rest.
/// </summary>
public interface IProviderSessionStore
{
    Task<CachedToken?> GetToken(string tenantId, CancellationToken cancellationToken);

    Task SaveToken(string tenantId, string token, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken);
}
