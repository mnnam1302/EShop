using EShop.Shared.DomainTools.Exceptions;
using Stateless;

namespace EShop.Order.Domain.StateMachines;

public sealed class OrderStateMachine : StateMachine<OrderState, OrderAction>
{
    public OrderStateMachine() : base(OrderState.Pending)
    {
    }

    public OrderStateMachine(Func<OrderState> stateAccessor, Action<OrderState> stateMutator)
        : base(stateAccessor, stateMutator)
    {
        Configure();
    }

    private void Configure()
    {
        Configure(OrderState.Pending)
            .Permit(OrderAction.Accept, OrderState.Accepted)
            .Permit(OrderAction.Reject, OrderState.Rejected);

        Configure(OrderState.Accepted);
        Configure(OrderState.Rejected);
    }
}

public enum OrderState
{
    Pending,
    Rejected,
    Accepted
}

public enum OrderAction
{
    Accept,
    Reject,
}
