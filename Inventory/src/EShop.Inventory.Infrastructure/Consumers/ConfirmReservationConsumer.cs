using EShop.Inventory.Domain.Enums;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Inventory;
using Grpc.Core;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Infrastructure.Consumers;

public sealed class ConfirmReservationConsumer(
    InventoryDbContext dbContext,
    IUserDetailsProvider userDetailsProvider,
    ILogger<ConfirmReservationConsumer> logger) : IConsumer<ConfirmReservationCommand>
{
    public async Task Consume(ConsumeContext<ConfirmReservationCommand> context)
    {
        var command = context.Message;

        using var _ = userDetailsProvider.CreateSystemUserScope(
            command.TenantId, command.ActionUserId, command.ActionUserType);

        var reservation = await dbContext.Reservations
            .FirstOrDefaultAsync(r => r.Id == command.ReservationId && r.OrderId == command.OrderId,
             context.CancellationToken);

        if (reservation is null)
        {
            logger.LogWarning("ConfirmReservationCommand for Reservation {ReservationId}: not found — no-op.", command.ReservationId);
            return;
        }

        if (reservation.Status != nameof(ReservationStatus.Pending))
        {
            logger.LogInformation(
                "ConfirmReservationCommand for Reservation {ReservationId}: status is {Status} — skipping.",
                command.ReservationId, reservation.Status);
            return;
        }

        reservation.Confirm();
        await dbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Confirmed reservation {ReservationId} for Order {OrderId}.", command.ReservationId, command.OrderId);
    }
}
