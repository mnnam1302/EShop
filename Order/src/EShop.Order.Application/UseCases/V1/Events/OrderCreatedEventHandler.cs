using EShop.Order.Domain.Sagas;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.UseCases.V1.Events;

internal sealed class OrderCreatedEventHandler(
    IAggregateSagaStore aggregateSagaStore,
    ILogger<OrderCreatedEventHandler> logger,
    IUserDetailsProvider userDetailsProvider,
    ICommandBus commandBus) : ICommandHandler<OrderCreated>
{
    public async Task<Result> HandleAsync(OrderCreated command, CancellationToken cancellationToken)
    {
        var orderSagaId = OrderSagaId.FromOrderId(command.OrderId);

        logger.LogInformation(
            "Processing OrderCreated for Order {OrderId} with Saga {OrderSagaId}",
            command.OrderId, orderSagaId);

        var existingSaga = await aggregateSagaStore.LoadAggregateSagaAsync<OrderSaga>(orderSagaId, cancellationToken);

        if (!existingSaga.IsNew)
        {
            logger.LogWarning(
                "OrderSaga already exists for Order {OrderId} (Saga {OrderSagaId}). Idempotency guard triggered.",
                command.OrderId, orderSagaId);

            return Result.Success();
        }

        var orderSaga = OrderSaga.Create(orderSagaId, command, userDetailsProvider);
        await aggregateSagaStore.UpdateAggregateSagaAsync(orderSaga, cancellationToken);

        await orderSaga.PublishAsync(commandBus, cancellationToken);

        logger.LogInformation(
            "OrderSaga started for Order {OrderId} (Saga {OrderSagaId}).",
            command.OrderId, orderSagaId);

        return Result.Success();
    }
}
