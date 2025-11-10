using EShop.Authorization.API.Models;
using Reqnroll;

namespace EShop.Authorization.Tests.Users.Invite;

[Binding]
internal sealed class Steps
{
    [When("user {string} invites a new user with role {string} the following details")]
    public async Task WhenUserInvitesANewUserWithRoleTheFollowingDetails(string username, string roleName, DataTable dataTable)
    {
        var request = dataTable.CreateInstance<InviteUserRequest>();
        //var role = await
        //await stepContext.InviteUserAsync(request, roleName, username);
    }

    [Then("user {string} has following details")]
    public void ThenUserHasFollowingDetails(string username, DataTable dataTable)
    {
        throw new PendingStepException();
    }
}
