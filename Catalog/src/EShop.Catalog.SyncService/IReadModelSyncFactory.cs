namespace EShop.Catalog.SyncService;

public interface IReadModelSyncFactory
{
    IReadModelSyncEventHandler GetSyncEventHandler(); // IReadModelSyncEventHandler handles events and updates the read model
    IReadModelSyncDataReader<T> GetReader<T>() where T : class; // IReadModelSyncDataReader reads the read model
}
