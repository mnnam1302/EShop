using Stateless;

namespace EShop.Authorization.Domain.StateMachines;

public sealed class UserStateMachine : StateMachine<UserState, UserAction>
{
    public UserStateMachine(Func<UserState> stateAccessor, Action<UserState> stateMutator)
        : base(stateAccessor, stateMutator)
    {
        Configure();
    }

    public UserStateMachine() : base(UserState.PendingVerification)
    {
        Configure();
    }

    public void Configure()
    {
        Configure(UserState.PendingVerification)
            .Permit(UserAction.Activate, UserState.Active)
            .Permit(UserAction.Delete, UserState.Deleted);

        Configure(UserState.Active)
            .Permit(UserAction.Lock, UserState.Locked)
            .Permit(UserAction.Delete, UserState.Deleted);

        Configure(UserState.Locked)
            .Permit(UserAction.Unlock, UserState.Active)
            .Permit(UserAction.Delete, UserState.Deleted);
    }
}

public enum UserState
{
    PendingVerification, // newly created, pending email verification
    Active,     // normal active state
    Inactive,   // deactivated by admin
    Locked,     // temporary lock because of too many failed login attempts
    Suspended,  // temporary disable because of policy violations, etc. 
    Deleted     // soft delete
}

public enum UserAction
{
    Activate,
    Deactivate,
    Lock,
    Unlock,
    Delete
}