using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.ReadModel.EfCore;

/// <summary>
/// EF Core implementation of <see cref="IReadModelStore{TReadModel}"/>.
/// Each write operation persists changes immediately via <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.
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

    public async Task InsertAsync(TReadModel readModel, CancellationToken cancellationToken = default)
    {
        _dbContext.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TReadModel readModel, CancellationToken cancellationToken = default)
    {
        _dbContext.Update(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is not null)
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
