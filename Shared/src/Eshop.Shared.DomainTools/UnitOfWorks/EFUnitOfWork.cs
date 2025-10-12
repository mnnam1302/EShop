using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DomainTools.UnitOfWorks;

public class EFUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EFUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}