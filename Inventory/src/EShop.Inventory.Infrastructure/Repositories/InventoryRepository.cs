using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.DomainTools.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShop.Inventory.Infrastructure.Repositories;

internal sealed class InventoryRepository
    : RepositoryBase<InventoryDbContext, Domain.Aggregates.Inventory, Guid>,
    IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task DecreaseStockLevel1(Guid variantId, int quantity, CancellationToken cancellationToken)
    {
        FormattableString rawSql = $"""
            UPDATE "Inventories"
            SET "StockAvailable" = "StockAvailable" - {quantity}
            WHERE "VariantId" = {variantId} 
              AND "StockAvailable" >= {quantity}
            """;

        await _dbContext.Database.ExecuteSqlAsync(rawSql, cancellationToken);
    }

    public async Task DecreaseStockLevel3CAS(Guid variantId, int oldStockAvailable, int quantity, CancellationToken cancellationToken)
    {
        FormattableString rawSql = $"""
            UPDATE "Inventories"
            SET "StockAvailable" = "StockAvailable" - {quantity}
            WHERE "VariantId" = {variantId} 
              AND "StockAvailable" >= {quantity}
            """;

        await _dbContext.Database.ExecuteSqlAsync(rawSql, cancellationToken);
    }
}
