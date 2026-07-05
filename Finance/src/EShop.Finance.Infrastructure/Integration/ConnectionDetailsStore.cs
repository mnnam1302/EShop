using System.Text.Json;
using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Security;
using Microsoft.EntityFrameworkCore;

namespace EShop.Finance.Infrastructure.Integration;

internal sealed class ConnectionDetailsStore : IConnectionDetailsStore
{
    private readonly FinanceDbContext _dbContext;
    private readonly IFieldEncryptor _encryptor;

    public ConnectionDetailsStore(FinanceDbContext dbContext, IFieldEncryptor encryptor)
    {
        _dbContext = dbContext;
        _encryptor = encryptor;
    }

    public async Task<IReadOnlyDictionary<string, string?>?> GetConnectionDetails(string tenantId, CancellationToken cancellationToken)
    {
        var accountingCompany = await _dbContext.AccountingCompanies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);

        if (string.IsNullOrEmpty(accountingCompany?.EncryptedConnectionDetails))
        {
            return null;
        }

        var json = _encryptor.Decrypt(accountingCompany.EncryptedConnectionDetails);
        return JsonSerializer.Deserialize<Dictionary<string, string?>>(json);
    }

    public async Task SaveConnectionDetails(string tenantId, IReadOnlyDictionary<string, string?> connectionDetails, CancellationToken cancellationToken)
    {
        var accountingCompany = await _dbContext.AccountingCompanies.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
        if (accountingCompany is null)
        {
            throw new InvalidOperationException($"No accounting company exists for tenant '{tenantId}'.");
        }

        var json = JsonSerializer.Serialize(connectionDetails);
        accountingCompany.SetConnectionDetails(_encryptor.Encrypt(json));

        _dbContext.AccountingCompanies.Update(accountingCompany);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
