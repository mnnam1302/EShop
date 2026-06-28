using EShop.Order.Domain.Sagas;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

public sealed class InventoryReservedConsumer(
    IAggregateSagaStore aggregateSagaStore,
    ILogger<InventoryReservedConsumer> logger,
    ICommandDispatcher commandDispatcher,
    ICommandBus commandBus,
    IUserDetailsProvider userDetailsProvider) : IConsumer<InventoryReserved>
{
    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        var message = context.Message;
        var sagaId = OrderSagaId.FromOrderId(message.OrderId);

        logger.LogInformation("InventoryReservedConsumer: Processing for Order {OrderId}, Saga {SagaId}", message.OrderId, sagaId);

        var saga = await aggregateSagaStore.LoadAggregateSagaAsync<OrderSaga>(sagaId, context.CancellationToken);

        if (saga.IsNew)
        {
            logger.LogWarning("InventoryReservedConsumer: Saga not found for Order {OrderId}", message.OrderId);
            return;
        }

        if (saga.IsCompleted())
        {
            logger.LogDebug("InventoryReservedConsumer: Saga {SagaId} already completed — duplicate delivery, skipping.", sagaId);
            return;
        }

        
        saga.HandleAsync(message, userDetailsProvider.AuthenticatedUser);
        await aggregateSagaStore.UpdateAggregateSagaAsync(saga, context.CancellationToken);

        await saga.PublishAsync(commandDispatcher, context.CancellationToken);
        await saga.PublishAsync(commandBus, context.CancellationToken);

        logger.LogInformation("InventoryReservedConsumer: Saga advanced for Order {OrderId}", message.OrderId);
    }
}
