using EShop.Inventory.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.HostedServices;

/// <summary>
/// Runs once at startup to ensure Redis stock counters are seeded from Postgres.
/// Skipped if the sentinel key already exists (e.g., rolling restart scenario).
/// </summary>
internal sealed class RedisStockInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisStockInitializer> _logger;

    public RedisStockInitializer(
        IServiceScopeFactory scopeFactory,
        ILogger<RedisStockInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var gateway = scope.ServiceProvider.GetRequiredService<IRedisStockGateway>();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        if (await gateway.IsInitializedAsync(cancellationToken))
        {
            _logger.LogInformation("Redis stock already initialized — skipping seed.");
            return;
        }

        _logger.LogInformation("Seeding Redis stock counters from Postgres…");

        var inventories = await dbContext.Inventories
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var inv in inventories)
        {
            await gateway.SeedStockAsync(inv.VariantId, inv.StockAvailable, cancellationToken);
        }

        // Set the sentinel so future restarts skip the seed.
        var db = scope.ServiceProvider.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>()
            .GetDatabase();
        await db.StringSetAsync("stock:_initialized", "1");

        _logger.LogInformation("Redis stock seeded for {Count} variants.", inventories.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
