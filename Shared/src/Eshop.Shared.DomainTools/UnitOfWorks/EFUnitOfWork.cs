using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DomainTools.UnitOfWorks;

public class EFUnitOfWork<TDbContext>(TDbContext dbContext) : IUnitOfWork where TDbContext : DbContext
{
    public async ValueTask DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
