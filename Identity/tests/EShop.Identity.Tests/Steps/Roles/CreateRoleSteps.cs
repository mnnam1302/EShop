using EShop.Identity.Tests.Steps.StepContext;
using Reqnroll;

namespace EShop.Identity.Tests.Steps.Roles;

[Binding]
internal class CreateRoleSteps
{
    private readonly RoleContext _roleContext;

    public CreateRoleSteps(RoleContext roleContext)
    {
        _roleContext = roleContext;
    }

    [When("user '(.*)' create role '(.*)'")]
    public async Task WhenUserCreateRole(string creatorUsername = null, string roleName= null)
    {
        _roleContext.Name = roleName ?? _roleContext.Name;
        await _roleContext.CreateRoleAsync(creatorUsername);
    }

    [Then("there are following Roles in the system")]
    public void ThenThereAreFollowingRolesInTheSystem(DataTable dataTable)
    {
        var test = 10;
    }

}