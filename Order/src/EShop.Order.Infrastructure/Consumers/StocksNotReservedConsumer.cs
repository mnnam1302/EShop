using EShop.Order.Domain.Sagas;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

internal sealed class StocksNotReservedConsumer(
    IAggregateSagaStore aggregateSagaStore,
    ILogger<StocksNotReservedConsumer> logger,
    ICommandDispatcher commandDispatcher) : IConsumer<StocksNotReserved>
{
    public async Task Consume(ConsumeContext<StocksNotReserved> context)
    {
        var message = context.Message;
        var sagaId = OrderSagaId.FromOrderId(message.OrderId).GetGuid();

        logger.LogWarning("StockReservationFailedConsumer: Stock reservation failed for Order {OrderId}, Saga {SagaId}. Reason: {Reason}",
            message.OrderId, sagaId, message.FailureReason);

        var saga = await aggregateSagaStore.LoadAggregateSagaAsync<OrderSaga>(sagaId, context.CancellationToken);

        if (saga.IsNew)
        {
            logger.LogWarning("StockReservationFailedConsumer: Saga not found for Order {OrderId}", message.OrderId);
            return;
        }

        saga.HandleAsync(message);
        await aggregateSagaStore.UpdateAggregateSagaAsync(saga, context.CancellationToken);

        await saga.PublishAsync(commandDispatcher, context.CancellationToken);

        logger.LogInformation("StockReservationFailedConsumer: Saga marked as failed for Order {OrderId}", message.OrderId);
    }
}
