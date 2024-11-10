using EShop.Identity.Domain.Abstractions.UnitOfWorks;

namespace EShop.Identity.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly UserDbContext _dbContext;

    public UnitOfWork(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
        => await _dbContext.DisposeAsync();

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}