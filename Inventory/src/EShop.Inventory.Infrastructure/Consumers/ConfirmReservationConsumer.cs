using EShop.Inventory.Domain.Enums;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Inventory;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="ConfirmReservationCommand"/> on payment success.
/// Moves Pending → Confirmed; no stock change (stock was deducted at placement).
/// </summary>
internal sealed class ConfirmReservationConsumer(
    InventoryDbContext dbContext,
    IUserDetailsProvider userDetailsProvider,
    ILogger<ConfirmReservationConsumer> logger) : IConsumer<ConfirmReservationCommand>
{
    public async Task Consume(ConsumeContext<ConfirmReservationCommand> context)
    {
        var cmd = context.Message;

        using var _ = userDetailsProvider.CreateSystemUserScope(
            cmd.TenantId, cmd.ActionUserId, cmd.ActionUserType);

        var reservation = await dbContext.Reservations
            .FirstOrDefaultAsync(
                r => r.Id == cmd.ReservationId && r.OrderId == cmd.OrderId && r.TenantId == cmd.TenantId,
                context.CancellationToken);

        if (reservation is null)
        {
            logger.LogWarning("ConfirmReservationCommand for Reservation {ReservationId}: not found — no-op.", cmd.ReservationId);
            return;
        }

        // Idempotent: already confirmed, released, or expired — no-op.
        if (reservation.Status != nameof(ReservationStatus.Pending))
        {
            logger.LogInformation("ConfirmReservationCommand for Reservation {ReservationId}: status is {Status} — skipping.", cmd.ReservationId, reservation.Status);
            return;
        }

        reservation.Confirm();
        await dbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Confirmed reservation {ReservationId} for Order {OrderId}.", cmd.ReservationId, cmd.OrderId);
    }
}
