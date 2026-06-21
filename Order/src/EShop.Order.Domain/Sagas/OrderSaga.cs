using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Sagas.DomainEvents;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.Authentication.Abstractions;
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
    public Guid ReservationId { get; private set; }

    public OrderSagaStates Status { get; private set; }

    public string TenantId { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;

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

        orderSaga.Publish(new MakeReservation
        {
            OrderId = orderSaga.OrderId,
            Items = message.Items,
            TenantId = orderSaga.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        });

        return orderSaga;
    }

    public void HandleAsync(StocksReserved message)
    {
        if (State != SagaState.Running)
        {
            throw new DomainException("OrderSaga", "StockReserved event received in wrong saga state");
        }

        RaiseEvent(new StockReservedEvent
        {
            ReservationId = message.ReservationId
        });

        Publish(new AcceptOrderCommand
        {
            OrderId = OrderId
        });
    }

    public void HandleAsync(StocksNotReserved message)
    {
        if (State != SagaState.Running)
        {
            throw new DomainException("OrderSaga", "StockReservationFailed event received in wrong saga state");
        }

        RaiseEvent(new StockReservationFailedEvent());

        Publish(new RejectOrderCommand
        {
            OrderId = OrderId,
            Reason = message.FailureReason
        });

        MarkComplete();
    }

    public void Apply(OrderSagaStartedEvent @event)
    {
        Id = @event.OrderSagaId;
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        Status = OrderSagaStates.AwaitingStockReservation;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    public void Apply(StockReservedEvent @event)
    {
        ReservationId = @event.ReservationId;
        Status = OrderSagaStates.StocksAccepted;
    }

    public void Apply(StockReservationFailedEvent _)
    {
        Status = OrderSagaStates.StocksRejected;
    }
}
