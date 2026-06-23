using EShop.Shared.DomainTools.Exceptions;
using Stateless;

namespace EShop.Order.Domain.StateMachines;

public sealed class OrderStateMachine : StateMachine<OrderState, OrderAction>
{
    public OrderStateMachine() : base(OrderState.ReservingInventory)
    {
        Configure();
    }

    public OrderStateMachine(Func<OrderState> stateAccessor, Action<OrderState> stateMutator)
        : base(stateAccessor, stateMutator)
    {
        Configure();
    }

    private void Configure()
    {
        OnUnhandledTrigger((state, trigger) =>
            throw new DomainException("Order", $"Action '{trigger}' is not permitted in state '{state}'."));

        Configure(OrderState.ReservingInventory)
            .Permit(OrderAction.StartPayment, OrderState.ProcessingPayment)
            .Permit(OrderAction.Reject, OrderState.Rejected);

        Configure(OrderState.ProcessingPayment)
            .Permit(OrderAction.Accept, OrderState.Accepted)
            .Permit(OrderAction.Reject, OrderState.Rejected);

        Configure(OrderState.Accepted);
        Configure(OrderState.Rejected);
    }
}

public enum OrderState
{
    ReservingInventory,
    ProcessingPayment,
    Rejected,
    Accepted
}

public enum OrderAction
{
    StartPayment,
    Accept,
    Reject,
}
