using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Identity.Domain.ActionPopulators;

internal interface IActionPopulator
{
    Task<Dictionary<string, ActionDefinition>> PopulateActions(
        IPermissionValidator permissionValidator,
        IStateTransitionController stateTransition,
        IFeatureValidator featureValidator);
}
