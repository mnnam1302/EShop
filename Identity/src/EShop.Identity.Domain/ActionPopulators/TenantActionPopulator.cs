using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureConstants;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Identity.Domain.ActionPopulators;

internal class TenantActionPopulator : IActionPopulator
{
    public async Task<Dictionary<string, ActionDefinition>> PopulateActions(
        IPermissionValidator permissionValidator,
        IStateTransitionController stateTransition,
        IFeatureValidator featureValidator)
    {
        return new Dictionary<string, ActionDefinition>
        {
            {
                nameof(UserTenantActions.ViewSystemSettings),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Tenancy.SystemFormatConfiguration_FeatureId) &&
                    await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        PermissionConstants.Tenancy.ViewSystemSettings,
                        PermissionConstants.Tenancy.ManageSystemSettings))
            },
            {
                nameof(UserTenantActions.ManageSystemSettings),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Tenancy.SystemFormatConfiguration_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.Tenancy.ManageSystemSettings))
            }
        };
    }
}