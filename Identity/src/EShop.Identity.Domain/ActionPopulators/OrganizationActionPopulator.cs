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
                    await featureValidator.HasFeatureAsync(IdentityFeatures.OrganisationRingFencing_FeatureId) &&
                    await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        IdentityPermissions.ViewOrganizationsPermissionId,
                        IdentityPermissions.ManageOrganizationsPermissionId))
            },
            {
                UserOrganizationActions.ManageOrganizations.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(IdentityFeatures.OrganisationRingFencing_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(IdentityPermissions.ManageOrganizationsPermissionId))
            }
        };
    }
}