using EShop.Inventory.Application.Services;
using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Enums;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Order.Saga;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.Consumers;

public sealed class ReleaseReservationConsumer(
    InventoryDbContext dbContext,
    IInventoryRepository inventoryRepository,
    IStockCacheService redisGateway,
    IUserDetailsProvider userDetailsProvider,
    ILogger<ReleaseReservationConsumer> logger) : IConsumer<ReleaseReservationCommand>
{
    public async Task Consume(ConsumeContext<ReleaseReservationCommand> context)
    {
        var command = context.Message;

        using var _ = userDetailsProvider.CreateSystemUserScope(command.TenantId, command.ActionUserId, command.ActionUserType);

        var reservation = await dbContext.Reservations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == command.ReservationId && r.OrderId == command.OrderId, context.CancellationToken);

        if (reservation is null)
        {
            logger.LogWarning("ReleaseReservationCommand for Reservation {ReservationId} / Order {OrderId}: not found — no-op.", command.ReservationId, command.OrderId);
            return;
        }

        if (reservation.Status != nameof(ReservationStatus.Pending))
        {
            logger.LogInformation("ReleaseReservationCommand for Reservation {ReservationId}: status is {Status} — skipping.", command.ReservationId, reservation.Status);
            return;
        }

        await using var tx = await dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var redisItems = new List<StockReservationRequest>(reservation.Items.Count);

            foreach (var item in reservation.Items)
            {
                // Atomic add-back (UPDATE ... SET stock_available = stock_available + qty) so concurrent
                // releases of the same variant cannot lose updates — mirrors the CAS deduct on placement.
                await inventoryRepository.AddBackStockAsync(item.VariantId, command.TenantId, item.Quantity, context.CancellationToken);

                redisItems.Add(new StockReservationRequest { VariantId = item.VariantId, Quantity = item.Quantity });
            }

            reservation.Release();
            await dbContext.SaveChangesAsync(context.CancellationToken);
            await tx.CommitAsync(context.CancellationToken);

            // Redis compensation — best-effort after Postgres commit.
            await redisGateway.ReleaseAsync(redisItems, context.CancellationToken);

            logger.LogInformation("Released reservation {ReservationId} for Order {OrderId} ({Count} items).", command.ReservationId, command.OrderId, reservation.Items.Count);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(context.CancellationToken);
            logger.LogError(ex, "Error releasing reservation {ReservationId} for Order {OrderId}.", command.ReservationId, command.OrderId);
            throw;
        }
    }
}
