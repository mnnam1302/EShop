using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureConstants;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Identity.Domain.ActionPopulators;

internal class UserActionPopulator : IActionPopulator
{
    public async Task<Dictionary<string, ActionDefinition>> PopulateActions(
        IPermissionValidator permissionValidator,
        IStateTransitionController stateTransition,
        IFeatureValidator featureValidator)
    {
        return new Dictionary<string, ActionDefinition>
        {
            {
                nameof(UserActions.ViewUsers),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(IdentityFeatures.UserInvites_FeatureId) &&
                    await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        IdentityPermissions.ViewUsersPermissionId,
                        IdentityPermissions.ManageUsersPermissionId))
            },
            {
                nameof(UserActions.InviteUser),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(IdentityFeatures.UserInvites_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(IdentityPermissions.ManageUsersPermissionId))
            },
            {
                UserActions.EditUser.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(IdentityFeatures.UserInvites_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(IdentityPermissions.ManageUsersPermissionId))
            },
            {
                UserActions.DeleteUser.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(IdentityFeatures.UserInvites_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(IdentityPermissions.ManageUsersPermissionId))
            },
            {
                UserActions.AssignRoles.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(IdentityFeatures.UserInvites_FeatureId) &&
                    await permissionValidator.HasPermissionAsync(IdentityPermissions.ManageUsersPermissionId))
            },
        };
    }
}