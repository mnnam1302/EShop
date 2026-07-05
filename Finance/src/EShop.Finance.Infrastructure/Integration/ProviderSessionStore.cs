using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Finance.Application.Services.IntegrationProvider.Security;
using Microsoft.EntityFrameworkCore;

namespace EShop.Finance.Infrastructure.Integration;

internal sealed class ProviderSessionStore : IProviderSessionStore
{
    private readonly FinanceDbContext dbContext;
    private readonly IFieldEncryptor encryptor;

    public ProviderSessionStore(FinanceDbContext dbContext, IFieldEncryptor encryptor)
    {
        this.dbContext = dbContext;
        this.encryptor = encryptor;
    }

    public async Task<CachedToken?> GetToken(string tenantId, CancellationToken cancellationToken)
    {
        var session = await dbContext.IntegrationProviderSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        if (string.IsNullOrEmpty(session?.SessionToken))
        {
            return null;
        }

        return new CachedToken(encryptor.Decrypt(session.SessionToken), session.ExpiresAtUtc);
    }

    public async Task SaveToken(string tenantId, string token, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
    {
        var session = await dbContext.IntegrationProviderSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        var encrypted = encryptor.Encrypt(token);

        if (session is null)
        {
            dbContext.IntegrationProviderSessions.Add(new IntegrationProviderSession
            {
                TenantId = tenantId,
                SessionToken = encrypted,
                ExpiresAtUtc = expiresAtUtc,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            session.SessionToken = encrypted;
            session.ExpiresAtUtc = expiresAtUtc;
            session.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
