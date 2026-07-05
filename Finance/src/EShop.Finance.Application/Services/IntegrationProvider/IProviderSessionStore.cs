using EShop.Finance.Application.Services.IntegrationProvider.Models;

namespace EShop.Finance.Application.Services.IntegrationProvider;

public interface IProviderSessionStore
{
    Task<CachedToken?> GetToken(string tenantId, CancellationToken cancellationToken);

    Task SaveToken(string tenantId, string token, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken);
}
