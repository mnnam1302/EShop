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

    public async Task<int> DeductStocLevel1Async(Guid variantId, string tenantId, int quantity, CancellationToken cancellationToken)
    {
        FormattableString sql = $"""
            UPDATE "Inventories"
            SET "StockAvailable" = "StockAvailable" - {quantity},
                "ReservedStock"  = "ReservedStock"  + {quantity}
            WHERE "VariantId" = {variantId}
              AND "StockAvailable" >= {quantity}
            """;

        return await _dbContext.Database.ExecuteSqlAsync(sql, cancellationToken);
    }

    public async Task AddBackStockAsync(Guid variantId, string tenantId, int quantity, CancellationToken cancellationToken)
    {
        FormattableString sql = $"""
            UPDATE "Inventories"
            SET "StockAvailable" = "StockAvailable" + {quantity},
                "ReservedStock"  = GREATEST(0, "ReservedStock" - {quantity})
            WHERE "VariantId" = {variantId}
            """;

        await _dbContext.Database.ExecuteSqlAsync(sql, cancellationToken);
    }
}
