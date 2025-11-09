using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureConstants;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Identity.Domain.ActionPopulators;

internal class OrganizationActionPopulator : IActionPopulator
{
    public async Task<Dictionary<string, ActionDefinition>> PopulateActions(
        IPermissionValidator permissionValidator,
        IStateTransitionController stateTransition,
        IFeatureValidator featureValidator)
    {
        return new Dictionary<string, ActionDefinition>
        {
            {
                nameof(UserOrganizationActions.ViewOrganizations),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Authorization.OrganisationRingFencing) &&
                    await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        PermissionConstants.Authorization.ViewOrganizations,
                        PermissionConstants.Authorization.ManageOrganizations))
            },
            {
                UserOrganizationActions.ManageOrganizations.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Authorization.OrganisationRingFencing) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.Authorization.ManageOrganizations))
            }
        };
    }
}