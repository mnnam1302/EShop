using EShop.Inventory.Application.Services;
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
internal sealed class ReleaseReservationConsumer(
    InventoryDbContext dbContext,
    IStockCacheService redisGateway,
    IUserDetailsProvider userDetailsProvider,
    ILogger<ReleaseReservationConsumer> logger) : IConsumer<ReleaseReservationCommand>
{
    public async Task Consume(ConsumeContext<ReleaseReservationCommand> context)
    {
        var cmd = context.Message;

        using var _ = userDetailsProvider.CreateSystemUserScope(
            cmd.TenantId, cmd.ActionUserId, cmd.ActionUserType);

        var reservation = await dbContext.Reservations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(
                r => r.Id == cmd.ReservationId && r.OrderId == cmd.OrderId && r.TenantId == cmd.TenantId,
                context.CancellationToken);

        if (reservation is null)
        {
            logger.LogWarning("ReleaseReservationCommand for Reservation {ReservationId} / Order {OrderId}: not found — no-op.", cmd.ReservationId, cmd.OrderId);
            return;
        }

        // Idempotent: already released or expired — no stock change, just ACK.
        if (reservation.Status != nameof(ReservationStatus.Pending))
        {
            logger.LogInformation("ReleaseReservationCommand for Reservation {ReservationId}: status is {Status} — skipping.", cmd.ReservationId, reservation.Status);
            return;
        }

        await using var tx = await dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var redisItems = new List<StockReservationRequest>(reservation.Items.Count);

            foreach (var item in reservation.Items)
            {
                var inventory = await dbContext.Inventories
                    .Where(i => i.VariantId == item.VariantId && i.TenantId == cmd.TenantId)
                    .FirstOrDefaultAsync(context.CancellationToken);

                if (inventory is not null)
                {
                    inventory.StockAvailable += item.Quantity;
                    inventory.ReservedStock = Math.Max(0, inventory.ReservedStock - item.Quantity);
                }

                redisItems.Add(new StockReservationRequest { VariantId = item.VariantId, Quantity = item.Quantity });
            }

            reservation.Release();
            await dbContext.SaveChangesAsync(context.CancellationToken);
            await tx.CommitAsync(context.CancellationToken);

            // Redis compensation — best-effort after Postgres commit.
            await redisGateway.ReleaseAsync(redisItems, context.CancellationToken);

            logger.LogInformation("Released reservation {ReservationId} for Order {OrderId} ({Count} items).", cmd.ReservationId, cmd.OrderId, reservation.Items.Count);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(context.CancellationToken);
            logger.LogError(ex, "Error releasing reservation {ReservationId} for Order {OrderId}.", cmd.ReservationId, cmd.OrderId);
            throw;
        }
    }
}
