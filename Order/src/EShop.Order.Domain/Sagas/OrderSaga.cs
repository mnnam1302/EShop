using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Sagas.DomainEvents;
using EShop.Order.Domain.StateMachines;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;

namespace EShop.Order.Domain.Sagas;

public sealed class OrderSaga : AggregateSaga, IScoped
{
    public string BuyerId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid ReservationId { get; private set; }

    public OrderSagaStateMachine State { get; private set; } = new();

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

        orderSaga.RaiseEvent(new SagaStartedEvent
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

    public void HandleAsync(InventoryReserved message, UserData currentUser)
    {
        if (!State.CanFire(OrderSagaTrigger.InventoryReserved))
        {
            throw new DomainException("OrderSaga", $"Cannot handle InventoryReserved in saga state '{State}'.");
        }

        RaiseEvent(new SagaInventoryReservedEvent { ReservationId = message.ReservationId });

        Publish(new StartOrderPaymentCommand { OrderId = OrderId });
        Publish(new MakePayment
        {
            OrderId = OrderId,
            BuyerId = BuyerId,
            TotalAmount = 1200,
            Currency = "VND",
            TenantId = currentUser.TenantId,
            ActionUserId = currentUser.ActionUserId,
            ActionUserType = currentUser.ActionUserType
        });

        MarkComplete();
    }

    public void HandleAsync(InventoryReservationFailed message)
    {
        if (!State.CanFire(OrderSagaTrigger.InventoryReservationFailed))
        {
            throw new DomainException("OrderSaga", $"Cannot handle InventoryReservationFailed in saga state '{State}'.");
        }

        RaiseEvent(new SagaInventoryReservationFailedEvent());

        Publish(new RejectOrderCommand
        {
            OrderId = OrderId,
            Reason = message.FailureReason
        });

        MarkComplete();
    }

    public void HandleExpire()
    {
        if (!State.CanFire(OrderSagaTrigger.Expire))
        {
            throw new DomainException("OrderSaga", $"Cannot handle Expire in saga state '{State}'.");
        }

        RaiseEvent(new SagaExpiredEvent());

        Publish(new RejectOrderCommand
        {
            OrderId = OrderId,
            Reason = "Order reservation timed out — no stock confirmation received."
        });

        MarkComplete();
    }

    public void Apply(SagaStartedEvent @event)
    {
        Id = @event.OrderSagaId;
        BuyerId = @event.BuyerId;
        OrderId = @event.OrderId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    public void Apply(SagaInventoryReservedEvent @event)
    {
        State.Fire(OrderSagaTrigger.InventoryReserved);
        ReservationId = @event.ReservationId;
    }

    public void Apply(SagaInventoryReservationFailedEvent _)
    {
        State.Fire(OrderSagaTrigger.InventoryReservationFailed);
    }

    public void Apply(SagaExpiredEvent _)
    {
        State.Fire(OrderSagaTrigger.Expire);
    }
}
