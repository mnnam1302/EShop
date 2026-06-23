using EShop.Order.Domain.Sagas;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.BackgroundJobs;

public sealed class OrderSagaExpireJob(
    ILogger<OrderSagaExpireJob> logger,
    IAggregateSagaStore sagaStore,
    ICommandDispatcher commandDispatcher)
{
    public async Task Execute(Guid sagaId, Guid orderId, CancellationToken cancellationToken)
    {
        var saga = await sagaStore.LoadAggregateSagaAsync<OrderSaga>(sagaId, cancellationToken);

        if (saga.IsNew || saga.IsCompleted())
        {
            logger.LogDebug("Saga {SagaId} already completed or not found. Timeout is a no-op.", sagaId);
            return;
        }

        if (saga.State.IsInState(OrderSagaState.ReservingInventory))
        {
            logger.LogDebug("Saga {SagaId} is in state {State}. Not eligible for expiration.", sagaId, saga.State);
            return;
        }

        logger.LogInformation("Saga {SagaId} (Order {OrderId}) expires in state {State}.", sagaId, orderId, saga.State);

        saga.HandleExpire();
        await sagaStore.UpdateAggregateSagaAsync(saga, cancellationToken);

        await saga.PublishAsync(commandDispatcher, cancellationToken);
    }
}
