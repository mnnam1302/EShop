using EShop.Order.Domain.Sagas;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

/// <summary>
/// Finance replied that the order's payment schedule was created. Advances the saga out of
/// ProcessingPayment: accepts the order and confirms the inventory reservation.
/// </summary>
public sealed class OrderPaymentScheduledConsumer(
    IAggregateSagaStore aggregateSagaStore,
    ILogger<OrderPaymentScheduledConsumer> logger,
    ICommandDispatcher commandDispatcher,
    ICommandBus commandBus,
    IUserDetailsProvider userDetailsProvider) : IConsumer<OrderPaymentScheduled>
{
    public async Task Consume(ConsumeContext<OrderPaymentScheduled> context)
    {
        var message = context.Message;
        var sagaId = OrderSagaId.FromOrderId(message.OrderId);

        logger.LogInformation("OrderPaymentScheduledConsumer: Processing for Order {OrderId}, Saga {SagaId}", message.OrderId, sagaId);

        var saga = await aggregateSagaStore.LoadAggregateSagaAsync<OrderSaga>(sagaId, context.CancellationToken);

        if (saga.IsNew)
        {
            logger.LogWarning("OrderPaymentScheduledConsumer: Saga not found for Order {OrderId}", message.OrderId);
            return;
        }

        if (saga.IsCompleted())
        {
            logger.LogDebug("OrderPaymentScheduledConsumer: Saga {SagaId} already completed — duplicate delivery, skipping.", sagaId);
            return;
        }

        saga.HandleAsync(message, userDetailsProvider.AuthenticatedUser);
        await aggregateSagaStore.UpdateAggregateSagaAsync(saga, context.CancellationToken);

        await saga.PublishAsync(commandDispatcher, context.CancellationToken);
        await saga.PublishAsync(commandBus, context.CancellationToken);

        logger.LogInformation("OrderPaymentScheduledConsumer: Saga advanced for Order {OrderId}", message.OrderId);
    }
}
