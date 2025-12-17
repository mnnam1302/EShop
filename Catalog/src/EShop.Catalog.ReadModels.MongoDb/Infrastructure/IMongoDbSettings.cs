namespace EShop.Catalog.ReadModels.MongoDb.Infrastructure;

public interface IMongoDbSettings
{
    string ConnectionString { get; }
    string DatabaseName { get; }
}

public sealed class MongoDbSettings : IMongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "eshop-catalog";
}