using EShop.Order.Domain.Sagas;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.BackgroundJobs;

public sealed class OrderSagaTimeoutJob(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderSagaTimeoutJob> logger)
{
    public async Task Execute(Guid sagaId, Guid orderId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sagaStore = scope.ServiceProvider.GetRequiredService<IAggregateSagaStore>();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var saga = await sagaStore.LoadAggregateSagaAsync<OrderSaga>(sagaId, cancellationToken);

        if (saga.IsNew || saga.IsCompleted())
        {
            logger.LogDebug("Saga {SagaId} already completed or not found. Timeout is a no-op.", sagaId);
            return;
        }

        if (saga.State.IsInState(OrderSagaState.AwaitingStockReservation))
        {
            logger.LogDebug("Saga {SagaId} is in state {State}. Not eligible for timeout.", sagaId, saga.State);
            return;
        }

        logger.LogWarning(
            "Saga {SagaId} (Order {OrderId}) timed out in state {State}. Rejecting order.",
            sagaId, orderId, saga.State);

        saga.HandleTimeout();
        await sagaStore.UpdateAggregateSagaAsync(saga, cancellationToken);
        await saga.PublishAsync(commandDispatcher, cancellationToken);
    }
}
