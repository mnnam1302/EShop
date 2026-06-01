using EShop.Shared.Contracts.Services.Inventory;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

/// <summary>
/// Receives <see cref="StockReserved"/> from Inventory and advances the saga to
/// AwaitingOrderPersistence, sending <c>PersistOrderCommand</c> to the write side.
/// </summary>
internal sealed class StockReservedConsumer(
    ILogger<StockReservedConsumer> logger) : IConsumer<StockReserved>
{
    public async Task Consume(ConsumeContext<StockReserved> context)
    {
        var msg = context.Message;

        //var state = await repository.FindAsync(msg.OrderId, context.CancellationToken);

        //if (state is null)
        //{
        //    logger.LogWarning("StockReserved for unknown Order {OrderId} — discarding.", msg.OrderId);
        //    return;
        //}

        //var result = orchestrator.OnStockReserved(state, msg);

        //if (!result.Transitioned)
        //{
        //    logger.LogWarning("StockReserved for Order {OrderId} ignored — already in state {State}.", msg.OrderId, state.CurrentState);
        //    return;
        //}

        //await repository.SaveChangeAsync(state, context.CancellationToken);

        //foreach (var cmd in result.Commands)
        //    await context.Publish(cmd, cmd.GetType(), context.CancellationToken);

        //logger.LogInformation("Order {OrderId} saga → {State}.", msg.OrderId, state.CurrentState);
    }
}
