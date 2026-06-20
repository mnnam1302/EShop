using EShop.Inventory.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job (every 5 minutes) that re-seeds Redis stock counters
/// from Postgres, ensuring eventual consistency after any Redis eviction or
/// counter drift.
/// </summary>
public sealed class SyncRedisStockJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncRedisStockJob> _logger;

    public SyncRedisStockJob(
        IServiceScopeFactory scopeFactory,
        ILogger<SyncRedisStockJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var gateway = scope.ServiceProvider.GetRequiredService<IRedisStockGateway>();

        var inventories = await dbContext.Inventories
            .AsNoTracking()
            .ToListAsync();

        foreach (var inv in inventories)
        {
            await gateway.SeedStockAsync(inv.VariantId, inv.StockAvailable);
        }

        _logger.LogInformation("Redis stock re-synced for {Count} variants.", inventories.Count);
    }
}
