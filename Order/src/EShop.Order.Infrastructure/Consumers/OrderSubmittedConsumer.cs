using EShop.Shared.Contracts.Services.Order;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

internal sealed class OrderSubmittedConsumer(
    ILogger<OrderSubmittedConsumer> logger) : IConsumer<OrderSubmitted>
{
    public async Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var msg = context.Message;

        //var state = await repository.FindAsync(msg.OrderId, context.CancellationToken)
        //    ?? new PlaceOrderSagaState
        //    {
        //        CorrelationId = msg.OrderId,
        //        UpdatedAt = DateTimeOffset.UtcNow
        //    };

        //var result = orchestrator.OnOrderSubmitted(state, msg);

        //if (!result.Transitioned)
        //{
        //    logger.LogWarning("OrderSubmitted for Order {OrderId} ignored — already in state {State}.", msg.OrderId, state.CurrentState);
        //    return;
        //}

        //await repository.SaveChangeAsync(state, context.CancellationToken);

        //await DispatchSideEffects(context, result);

        //logger.LogInformation("Order {OrderId} saga started to {State}.", msg.OrderId, state.CurrentState);
    }

    //private static async Task DispatchSideEffects(ConsumeContext context, SagaTransitionResult result)
    //{
    //    foreach (var cmd in result.Commands)
    //    {
    //        await context.Publish(cmd, cmd.GetType(), context.CancellationToken);
    //    }

    //    foreach (var evt in result.Events)
    //    {
    //        await context.Publish(evt, evt.GetType(), context.CancellationToken);
    //    }
    //}
}
