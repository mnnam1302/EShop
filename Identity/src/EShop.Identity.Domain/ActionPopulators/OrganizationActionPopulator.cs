using EShop.Shared.Scoping.ResourceAccessControl;

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
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_OrganisationRingFencing_FeatureId)
                    && await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        PermissionConstants.ViewOrganizationsPermissionId,
                        PermissionConstants.ManageOrganizationsPermissionId))
            },
            {
                UserOrganizationActions.ManageOrganizations.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_OrganisationRingFencing_FeatureId)
                    && await permissionValidator.HasPermissionAsync(PermissionConstants.ManageOrganizationsPermissionId))
            }
        };
    }
}