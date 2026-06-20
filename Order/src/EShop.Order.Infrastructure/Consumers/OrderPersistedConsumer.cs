using EShop.Shared.Contracts.Services.Order.Saga;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

/// <summary>
/// Receives <see cref="OrderPersisted"/> published by <c>PersistOrderConsumer</c>
/// and completes the saga, publishing <c>OrderAccepted</c>.
/// </summary>
internal sealed class OrderPersistedConsumer(
    ILogger<OrderPersistedConsumer> logger) : IConsumer<OrderPersisted>
{
    public async Task Consume(ConsumeContext<OrderPersisted> context)
    {
        var msg = context.Message;

        //var state = await repository.FindAsync(msg.OrderId, context.CancellationToken);

        //if (state is null)
        //{
        //    logger.LogWarning("OrderPersisted for unknown Order {OrderId} — discarding.", msg.OrderId);
        //    return;
        //}

        //var result = orchestrator.OnOrderPersisted(state, msg);

        //if (!result.Transitioned)
        //{
        //    logger.LogWarning("OrderPersisted for Order {OrderId} ignored — already in state {State}.", msg.OrderId, state.CurrentState);
        //    return;
        //}

        //await repository.SaveChangeAsync(state, context.CancellationToken);

        //foreach (var evt in result.Events)
        //    await context.Publish(evt, evt.GetType(), context.CancellationToken);

        //logger.LogInformation("Order {OrderId} saga completed → {State}.", msg.OrderId, state.CurrentState);
    }
}
