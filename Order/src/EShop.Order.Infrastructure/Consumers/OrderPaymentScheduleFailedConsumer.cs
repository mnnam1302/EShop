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
/// Finance replied that the order's payment schedule could not be created. Compensates the saga:
/// rejects the order and releases the inventory reservation.
/// </summary>
public sealed class OrderPaymentScheduleFailedConsumer(
    IAggregateSagaStore aggregateSagaStore,
    ILogger<OrderPaymentScheduleFailedConsumer> logger,
    ICommandDispatcher commandDispatcher,
    ICommandBus commandBus,
    IUserDetailsProvider userDetailsProvider) : IConsumer<OrderPaymentScheduleFailed>
{
    public async Task Consume(ConsumeContext<OrderPaymentScheduleFailed> context)
    {
        var message = context.Message;
        var sagaId = OrderSagaId.FromOrderId(message.OrderId);

        logger.LogInformation("OrderPaymentScheduleFailedConsumer: Processing for Order {OrderId}, Saga {SagaId}", message.OrderId, sagaId);

        var saga = await aggregateSagaStore.LoadAggregateSagaAsync<OrderSaga>(sagaId, context.CancellationToken);

        if (saga.IsNew)
        {
            logger.LogWarning("OrderPaymentScheduleFailedConsumer: Saga not found for Order {OrderId}", message.OrderId);
            return;
        }

        if (saga.IsCompleted())
        {
            logger.LogDebug("OrderPaymentScheduleFailedConsumer: Saga {SagaId} already completed — duplicate delivery, skipping.", sagaId);
            return;
        }

        saga.HandleAsync(message, userDetailsProvider.AuthenticatedUser);
        await aggregateSagaStore.UpdateAggregateSagaAsync(saga, context.CancellationToken);

        await saga.PublishAsync(commandDispatcher, context.CancellationToken);
        await saga.PublishAsync(commandBus, context.CancellationToken);

        logger.LogInformation("OrderPaymentScheduleFailedConsumer: Saga compensated for Order {OrderId}", message.OrderId);
    }
}
