using EShop.Shared.Contracts.Services.Inventory;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

/// <summary>
/// Receives <see cref="StockReservationFailed"/> from Inventory and fails the saga,
/// publishing <c>OrderRejected</c>.
/// </summary>
internal sealed class StockReservationFailedConsumer(
    ILogger<StockReservationFailedConsumer> logger) : IConsumer<StockReservationFailed>
{
    public async Task Consume(ConsumeContext<StockReservationFailed> context)
    {
        var msg = context.Message;

        //var state = await repository.FindAsync(msg.OrderId, context.CancellationToken);

        //if (state is null)
        //{
        //    logger.LogWarning("StockReservationFailed for unknown Order {OrderId} — discarding.", msg.OrderId);
        //    return;
        //}

        //var result = orchestrator.OnStockReservationFailed(state, msg);

        //if (!result.Transitioned)
        //{
        //    logger.LogWarning("StockReservationFailed for Order {OrderId} ignored — already in state {State}.", msg.OrderId, state.CurrentState);
        //    return;
        //}

        //await repository.SaveChangeAsync(state, context.CancellationToken);

        //foreach (var evt in result.Events)
        //    await context.Publish(evt, evt.GetType(), context.CancellationToken);

        //logger.LogInformation("Order {OrderId} saga failed → {State}: {Reason}.", msg.OrderId, state.CurrentState, msg.Reason);
    }
}
