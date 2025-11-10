using EShop.Authorization.API.Models;
using Reqnroll;

namespace EShop.Authorization.Tests.Users.Invite;

[Binding]
internal sealed class Steps(StepContext stepContext, Roles.Get.StepContext roleContext)
{
    [When("user invites a new user with role {string} the following details")]
    public async Task WhenUserInvitesANewUserWithRoleTheFollowingDetails(string roleName, DataTable dataTable)
    {
        var role = await roleContext.GetByNameAsync(roleName);

        var request = dataTable.CreateInstance<InviteUserRequest>();
        request.RoleIds = [role.Id];

        await stepContext.InviteUserAsync(request);
    }


    [Then("user {string} has following details")]
    public void ThenUserHasFollowingDetails(string username, DataTable dataTable)
    {
        throw new PendingStepException();
    }
}
