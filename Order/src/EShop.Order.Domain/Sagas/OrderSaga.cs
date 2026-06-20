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


    public void HandleAsync(OrderCreated message)
    {
        if (State != SagaState.New)
        {
            throw new DomainException("OrderSaga", "Saga must be new");
        }
    }

    public void HandleAsync(StockReserved message)
    {
        if (State != SagaState.Running)
        {
            throw new DomainException("OrderSaga", "StockReserved event received in wrong saga state");
        }

        RaiseEvent(new StockReservedEvent
        {
            OrderSagaId = OrderId,
            BuyerId = BuyerId,
            OrderId = OrderId,
            TenantId = TenantId,
            Scope = Scope
        });
    }

    public void HandleAsync(StockReservationFailed message)
    {
        if (State != SagaState.Running)
        {
            throw new DomainException("OrderSaga", "StockReservationFailed event received in wrong saga state");
        }

        RaiseEvent(new StockReservationFailedEvent
        {
            OrderSagaId = OrderId,
            BuyerId = BuyerId,
            OrderId = OrderId,
            TenantId = TenantId,
            Scope = Scope,
            FailureReason = message.FailureReason
        });

        MarkComplete();
    }

    public void HandleAsync(OrderPersisted message)
    {
        if (State != SagaState.Running)
        {
            throw new DomainException("OrderSaga", "OrderPersisted event received in wrong saga state");
        }

        RaiseEvent(new OrderPersistedEvent
        {
            OrderSagaId = OrderId,
            BuyerId = BuyerId,
            OrderId = OrderId,
            TenantId = TenantId,
            Scope = Scope
        });

        MarkComplete();
    }

    public void Apply(OrderSagaStartedEvent @event)
    {
        Id = @event.OrderSagaId;
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    public void Apply(StockReservedEvent @event)
    {
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    public void Apply(StockReservationFailedEvent @event)
    {
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    public void Apply(OrderPersistedEvent @event)
    {
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }
}
