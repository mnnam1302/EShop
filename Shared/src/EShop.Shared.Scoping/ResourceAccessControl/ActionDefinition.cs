namespace EShop.Shared.Scoping.ResourceAccessControl;

public class ActionDefinition
{
    public ActionDefinition(bool isAllowed)
    {
        IsAllowed = isAllowed;
    }

    public bool IsAllowed { get; set; }

    public static async Task<ActionDefinition> Create(
        string actionName,
        IStateTransitionController stateTransition,
        IPermissionValidator permissionValidator,
        params string[] permissionIds)
    {
        bool isAllowed = stateTransition.IsAllowed(actionName);
        bool isPermitted = await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(permissionIds);

        return new ActionDefinition(isAllowed && isPermitted);
    }
}