namespace EShop.Catalog.ReadModels.MongoDb.Models;

public interface ITenantProvider
{
    string? TenantId { get; }
}
