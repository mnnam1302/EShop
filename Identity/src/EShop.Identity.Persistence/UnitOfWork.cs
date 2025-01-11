using EShop.Identity.Domain.Abstractions.UnitOfWorks;

namespace EShop.Identity.Persistence;

[Obsolete("This class is obsolete. Use IUnitOfWork instead in Domain.Tools to generic.")]
public class UnitOfWork : IUnitOfWork
{
    private readonly UsersDbContext _dbContext;

    public UnitOfWork(UsersDbContext dbContext)
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