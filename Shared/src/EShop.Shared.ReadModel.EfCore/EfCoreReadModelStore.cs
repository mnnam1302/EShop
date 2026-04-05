using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.ReadModel.EfCore;

/// <summary>
/// EF Core implementation of <see cref="IReadModelStore{TReadModel}"/>.
/// Tracks changes via the <see cref="DbContext"/> change tracker without calling
/// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>, allowing the caller
/// to batch persistence within a unit of work.
/// </summary>
/// <typeparam name="TReadModel">The read model entity type.</typeparam>
/// <typeparam name="TDbContext">The EF Core DbContext type.</typeparam>
public sealed class EfCoreReadModelStore<TReadModel, TDbContext> : IReadModelStore<TReadModel>
    where TReadModel : class, IReadModel
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EfCoreReadModelStore(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TReadModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TReadModel>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task Insert(TReadModel readModel, CancellationToken cancellationToken = default)
    {
        _dbContext.Add(readModel);
        await Task.CompletedTask;
    }

    public async Task Update(TReadModel readModel, CancellationToken cancellationToken = default)
    {
        _dbContext.Update(readModel);
        await Task.CompletedTask;
    }

    public async Task Delete(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is not null)
        {
            _dbContext.Remove(entity);
        }
    }
}
