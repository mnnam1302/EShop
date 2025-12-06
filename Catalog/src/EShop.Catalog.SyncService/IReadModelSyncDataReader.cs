namespace EShop.Catalog.SyncService;

public interface IReadModelSyncDataReader<TEntityReadModel> where TEntityReadModel : class
{
    Task<TEntityReadModel?> FindById(Guid id, CancellationToken cancellationToken = default);
}