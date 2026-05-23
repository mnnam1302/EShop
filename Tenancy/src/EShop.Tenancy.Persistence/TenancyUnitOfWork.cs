using EShop.Tenancy.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Persistence;

internal sealed class TenancyUnitOfWork : ITenancyUnitOfWork
{
    private readonly TenancyDbContext _dbContext;

    public TenancyUnitOfWork(TenancyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"SELECT set_config('app.tenant_id', {tenantId}, false);",
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}
