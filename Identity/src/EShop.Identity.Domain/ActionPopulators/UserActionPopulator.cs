using EShop.Shared.Scoping.ResourceAccessControl;

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
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_UserInvites_FeatureId)
                    && await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(
                        PermissionConstants.ViewUsersPermissionId,
                        PermissionConstants.ManageUsersPermissionId))
            },
            {
                nameof(UserActions.InviteUser),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_UserInvites_FeatureId)
                    && await permissionValidator.HasPermissionAsync(PermissionConstants.ManageUsersPermissionId))
            },
            {
                UserActions.EditUser.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_UserInvites_FeatureId)
                    && await permissionValidator.HasPermissionAsync(PermissionConstants.ManageUsersPermissionId))
            },
            {
                UserActions.DeleteUser.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_UserInvites_FeatureId)
                    && await permissionValidator.HasPermissionAsync(PermissionConstants.ManageUsersPermissionId))
            },
            {
                UserActions.AssignRoles.ToString(),
                new ActionDefinition(
                    await featureValidator.HasFeatureAsync(FeatureConstants.Identity_UserInvites_FeatureId)
                    && await permissionValidator.HasPermissionAsync(PermissionConstants.ManageUsersPermissionId))
            },
        };
    }
}