using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace EShop.Catalog.Tests.Setup;

public sealed class MongoDbTestDatabase
{
    private string databaseName = string.Empty;

    public required MongoDbContainer MongoDbContainer { get; set; }

    public string ConnectionString => MongoDbContainer.GetConnectionString();

    public string DatabaseName => databaseName;

    public void CreateDatabase()
    {
        databaseName = $"catalog_test_{Guid.NewGuid():N}";
    }

    public async Task DropAsync()
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            return;
        }

        var client = new MongoClient(ConnectionString);
        await client.DropDatabaseAsync(databaseName);
    }
}
