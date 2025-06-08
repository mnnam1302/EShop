namespace EShop.Shared.Scoping.ResourceAccessControl;

public interface IStateTransitionController
{
    bool IsAllowed(string targetTransition);
}