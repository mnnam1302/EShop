using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureIds;
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
                    await featureValidator.HasFeatureAsync(FeatureIds.Authorization.UserInvites) &&
                    await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        PermissionConstants.Authorization.ViewUsers,
                        PermissionConstants.Authorization.ManageUsers))
            },
            {
                nameof(UserActions.InviteUser),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureIds.Authorization.UserInvites) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.Authorization.ManageUsers))
            },
            {
                UserActions.EditUser.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureIds.Authorization.UserInvites) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.Authorization.ManageUsers))
            },
            {
                UserActions.DeleteUser.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureIds.Authorization.UserInvites) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.Authorization.ManageUsers))
            },
            {
                UserActions.AssignRoles.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureIds.Authorization.UserInvites) &&
                    await permissionValidator.HasPermissionAsync(PermissionConstants.Authorization.ManageUsers))
            },
        };
    }
}