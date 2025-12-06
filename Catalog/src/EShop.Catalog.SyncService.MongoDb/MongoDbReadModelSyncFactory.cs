namespace EShop.Catalog.SyncService.MongoDb;

public sealed class MongoDbReadModelSyncFactory : IReadModelSyncFactory
{
    public IReadModelSyncDataReader<T> GetReader<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public IReadModelSyncEventHandler GetSyncEventHandler()
    {
        throw new NotImplementedException();
    }
}