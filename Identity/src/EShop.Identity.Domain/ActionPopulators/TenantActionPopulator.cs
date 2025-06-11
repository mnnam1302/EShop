using EShop.Shared.Scoping.ResourceAccessControl;

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
                    await featureValidator.HasFeatureAsync(FeatureConstants.Tenancy_SystemFormatConfiguration_FeatureId) &&
                    await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        PermissionConstants.ViewSystemSettingsPermissionId,
                        PermissionConstants.ManageSystemSettingsPermissionId))
            },
            {
                nameof(UserTenantActions.ManageSystemSettings),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Tenancy_SystemFormatConfiguration_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.ManageSystemSettingsPermissionId))
            }
        };
    }
}