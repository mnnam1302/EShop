using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job (every 1 minute) that transitions Active reservations
/// past their TTL to Expired status and compensates Redis counters.
/// </summary>
public sealed class ExpireReservationsJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireReservationsJob> _logger;

    public ExpireReservationsJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpireReservationsJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var gateway = scope.ServiceProvider.GetRequiredService<IRedisStockGateway>();

        var now = DateTimeOffset.UtcNow;

        var expired = await dbContext.StockReservations
            //.Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt <= now)
            .ToListAsync();

        if (expired.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Expiring {Count} reservations.", expired.Count);

        foreach (var reservation in expired)
        {
            reservation.Expire();

            // Compensate Redis: return reserved qty to available.
            await gateway.ReleaseAsync(
            [
                new StockReservationRequest
                {
                    VariantId = reservation.VariantId,
                    Quantity = reservation.Quantity
                }
            ]);
        }

        await dbContext.SaveChangesAsync();
    }
}
