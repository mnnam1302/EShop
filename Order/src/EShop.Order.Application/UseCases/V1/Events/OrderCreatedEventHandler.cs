using EShop.Order.Domain.Sagas;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.UseCases.V1.Events;

internal sealed class OrderCreatedEventHandler : ICommandHandler<OrderCreated>
{
    private readonly IAggregateSagaStore _aggregateSagaStore;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(IAggregateSagaStore aggregateSagaStore, ILogger<OrderCreatedEventHandler> logger)
    {
        _aggregateSagaStore = aggregateSagaStore;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(OrderCreated command, CancellationToken cancellationToken)
    {
        var orderSagaId = OrderSagaId
            .FromOrderId(command.OrderId)
            .GetGuid();

        _logger.LogInformation(
            "Processing OrderCreated event for Order ID: {OrderId} with Saga ID: {OrderSagaId}",
            command.OrderId, orderSagaId);

        var existingSaga = await _aggregateSagaStore.LoadAggregateSagaAsync<OrderSaga>(orderSagaId, cancellationToken);

        if (!existingSaga.IsNew)
        {
            _logger.LogWarning(
                "Idempotency conflict detected. OrderSaga already exists for Order ID: {OrderId} (Saga ID: {OrderSagaId}). Command execution halted.",
                command.OrderId, orderSagaId);

            return Result.Success();
        }

        var orderSaga = OrderSaga.Create(orderSagaId, command);

        _logger.LogInformation(
            "Successfully initialized and persisted OrderSaga for Order ID: {OrderId} (Saga ID: {OrderSagaId})",
            command.OrderId, orderSagaId);

        await _aggregateSagaStore.UpdateAggregateSagaAsync(orderSaga, cancellationToken);

        return Result.Success();
    }
}
