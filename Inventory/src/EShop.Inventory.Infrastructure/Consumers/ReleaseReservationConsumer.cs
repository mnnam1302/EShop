using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Enums;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Order.Saga;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="ReleaseReservationCommand"/> sent by the PlaceOrder saga
/// during the compensation path (stock reservation released back to available).
/// </summary>
internal sealed class ReleaseReservationConsumer : IConsumer<ReleaseReservationCommand>
{
    private readonly InventoryDbContext _dbContext;
    private readonly IRedisStockGateway _redisGateway;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<ReleaseReservationConsumer> _logger;

    public ReleaseReservationConsumer(
        InventoryDbContext dbContext,
        IRedisStockGateway redisGateway,
        IUserDetailsProvider userDetailsProvider,
        ILogger<ReleaseReservationConsumer> logger)
    {
        _dbContext = dbContext;
        _redisGateway = redisGateway;
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReleaseReservationCommand> context)
    {
        var cmd = context.Message;

        using var _ = _userDetailsProvider.CreateSystemUserScope(
            cmd.TenantId, cmd.ActionUserId, cmd.ActionUserType);

        var reservations = await _dbContext.StockReservations
            .Where(r => r.OrderId == cmd.OrderId && r.Status == ReservationStatus.Active)
            .ToListAsync(context.CancellationToken);

        if (reservations.Count == 0)
        {
            _logger.LogWarning("ReleaseReservationCommand for Order {OrderId}: no active reservations found — idempotent no-op.", cmd.OrderId);
            return;
        }

        await using var tx = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var redisItems = new List<StockReservationRequest>(reservations.Count);

            foreach (var reservation in reservations)
            {
                var inventory = await _dbContext.Inventories
                    .Where(i => i.VariantId == reservation.VariantId && i.TenantId == cmd.TenantId)
                    .FirstOrDefaultAsync(context.CancellationToken);

                if (inventory is not null)
                {
                    inventory.StockAvailable += reservation.Quantity;
                    inventory.ReservedStock = Math.Max(0, inventory.ReservedStock - reservation.Quantity);
                }

                redisItems.Add(new StockReservationRequest
                {
                    VariantId = reservation.VariantId,
                    Quantity = reservation.Quantity
                });

                reservation.Release();
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await tx.CommitAsync(context.CancellationToken);

            // Redis compensation — best-effort after Postgres commit.
            await _redisGateway.ReleaseAsync(redisItems, context.CancellationToken);

            _logger.LogInformation("Released {Count} reservations for Order {OrderId}.", reservations.Count, cmd.OrderId);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(context.CancellationToken);
            _logger.LogError(ex, "Error releasing reservations for Order {OrderId}.", cmd.OrderId);
            throw;
        }
    }
}
