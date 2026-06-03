using EShop.Order.Domain.Sagas.DomainEvents;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Inventory;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.Sagas;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;

namespace EShop.Order.Domain.Sagas;

public sealed class OrderSaga : AggregateSaga, IScoped
{
    public string BuyerId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }

    public string TenantId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    public static OrderSaga Create(Guid orderSagaId, OrderCreated message, IUserDetailsProvider userDetailsProvider)
    {
        var orderSaga = new OrderSaga
        {
            Id = orderSagaId,
            BuyerId = message.BuyerId,
            OrderId = message.OrderId,
            TenantId = message.TenantId,
            Scope = message.TenantId
        };

        orderSaga.RaiseEvent(new OrderSagaStartedEvent
        {
            OrderSagaId = orderSagaId,
            BuyerId = orderSaga.BuyerId,
            OrderId = orderSaga.OrderId,
            TenantId = orderSaga.TenantId,
            Scope = orderSaga.Scope
        });

        orderSaga.Publish(new MakeReservationCommand
        {
            OrderId = orderSaga.OrderId,
            Items = message.Items,
            TenantId = orderSaga.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        });

        return orderSaga;
    }

    public void Apply(OrderSagaStartedEvent @event)
    {
        Id = @event.OrderSagaId;
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    /// <summary>
    /// OrderSubmitted
    /// Starts the saga: Initial → AwaitingStockReservation.
    /// Side effect: <see cref="ReserveStockCommand"/> sent to Inventory.
    /// </summary>
    public void HandleAsync(OrderCreated message)
    {
        if (State != SagaState.New)
        {
            throw new DomainException("OrderSaga", "Saga must be new.");
        }

        //state.CurrentState = SagaStates.AwaitingStockReservation;
        //state.BuyerId = message.BuyerId;
        //state.TenantId = message.TenantId;
        //state.ActionUserId = message.ActionUserId;
        //state.ActionUserType = message.ActionUserType;
        //state.OrderItemsJson = JsonSerializer.Serialize(message.Items);
        //state.SubmittedAt = message.SubmittedAt;
        //state.ReservationAttempts = 1;
        //state.UpdatedAt = DateTimeOffset.UtcNow;

        //return SagaTransitionResult
        //    .Success()
        //    .WithCommand(new ReserveStockCommand
        //    {
        //        OrderId = state.CorrelationId,
        //        IdempotencyKey = state.CorrelationId,
        //        Items = message.Items,
        //        TenantId = message.TenantId,
        //        ActionUserId = message.ActionUserId,
        //        ActionUserType = message.ActionUserType
        //    });
    }

    /// <summary>
    /// StockReserved
    /// Advances the saga: AwaitingStockReservation → AwaitingOrderPersistence.
    /// Side effect: <see cref="PersistOrderCommand"/> sent to the Order write-side.
    /// </summary>
    public void HandleAsync(StockReserved message)
    {
        //if (state.CurrentState != SagaStates.AwaitingStockReservation)
        //    return SagaTransitionResult.NoOp();

        //state.CurrentState = SagaStates.AwaitingOrderPersistence;
        //state.ReservationId = message.ReservationId;
        //state.UpdatedAt = DateTimeOffset.UtcNow;

        //var items = JsonSerializer.Deserialize<List<OrderItem>>(state.OrderItemsJson) ?? [];

        //return SagaTransitionResult.Success()
        //    .WithCommand(new PersistOrderCommand
        //    {
        //        OrderId = state.CorrelationId,
        //        BuyerId = state.BuyerId,
        //        ReservationId = message.ReservationId,
        //        Items = items,
        //        TenantId = state.TenantId,
        //        ActionUserId = state.ActionUserId,
        //        ActionUserType = state.ActionUserType
        //    });
    }

    /// <summary>
    /// StockReservationFailed
    /// Fails the saga: AwaitingStockReservation → Failed.
    /// Side effect: <see cref="OrderRejected"/> published.
    /// </summary>
    public void HandleAsync(StockReservationFailed message)
    {
        //if (state.CurrentState != SagaStates.AwaitingStockReservation)
        //    return SagaTransitionResult.NoOp();

        //state.CurrentState = SagaStates.Failed;
        //state.FailureReason = message.Reason;
        //state.UpdatedAt = DateTimeOffset.UtcNow;

        //return SagaTransitionResult.Success()
        //    .WithEvent(new OrderRejected
        //    {
        //        OrderId = state.CorrelationId,
        //        BuyerId = state.BuyerId,
        //        Reason = message.Reason,
        //        RejectedAt = DateTimeOffset.UtcNow,
        //        TenantId = state.TenantId,
        //        ActionUserId = state.ActionUserId,
        //        ActionUserType = state.ActionUserType
        //    });
    }

    /// <summary>
    /// OrderPersisted
    /// Completes the saga: AwaitingOrderPersistence → Completed.
    /// Side effect: <see cref="OrderAccepted"/> published.
    /// </summary>
    public void HandleAsync(OrderPersisted message)
    {
        //if (state.CurrentState != SagaStates.AwaitingOrderPersistence)
        //    return SagaTransitionResult.NoOp();

        //state.CurrentState = SagaStates.Completed;
        //state.UpdatedAt = DateTimeOffset.UtcNow;

        //return SagaTransitionResult.Success()
        //    .WithEvent(new OrderAccepted
        //    {
        //        OrderId = state.CorrelationId,
        //        BuyerId = state.BuyerId,
        //        AcceptedAt = message.PersistedAt,
        //        TenantId = state.TenantId,
        //        ActionUserId = state.ActionUserId,
        //        ActionUserType = state.ActionUserType
        //    });
    }
}
