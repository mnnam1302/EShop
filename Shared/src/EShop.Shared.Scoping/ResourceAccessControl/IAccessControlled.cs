namespace EShop.Shared.Scoping.ResourceAccessControl;

public interface IAccessControlled
{
    Dictionary<string, ActionDefinition> Actions { get; }

    Task PopulateActions(IPermissionValidator permissionValidator, IStateTransitionController stateTransition, IFeatureValidator featureValidator);
}