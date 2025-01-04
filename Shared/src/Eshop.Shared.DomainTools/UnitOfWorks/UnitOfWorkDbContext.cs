

using Microsoft.EntityFrameworkCore;

namespace Eshop.Shared.DomainTools.UnitOfWorks;

public class UnitOfWorkDbContext<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public UnitOfWorkDbContext(TDbContext dbContext)
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