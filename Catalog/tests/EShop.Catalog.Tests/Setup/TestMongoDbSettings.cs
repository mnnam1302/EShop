using EShop.Catalog.ReadModels.MongoDb.Infrastructure;

namespace EShop.Catalog.Tests.Setup;

internal sealed class TestMongoDbSettings(string connectionString, string databaseName) : IMongoDbSettings
{
    public string ConnectionString { get; } = connectionString;
    public string DatabaseName { get; } = databaseName;
}