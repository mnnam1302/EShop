using EShop.Identity.Tests.Steps.StepContext;
using Reqnroll;

namespace EShop.Identity.Tests.Steps.Users;

[Binding]
internal class CreateUserSteps
{
    private readonly UserContext _userContext;

    public CreateUserSteps(UserContext userContext)
    {
        _userContext = userContext;
    }

    [Given("following tenant users added to the system")]
    public async Task GivenFollowingTenantUsersAddedToTheSystem(DataTable dataTable)
    {
        foreach (var tenant in dataTable.Rows)
        {
            await _userContext.SimulateTenantUserCreationAsync(
                tenant["TenantName"],
                tenant["Username"],
                tenant["DisplayName"],
                tenant["Email"],
                tenant.ContainsKey("Group") ? tenant["Group"] : string.Empty,
                !tenant.ContainsKey("SetAsDefault") || bool.Parse(tenant["SetAsDefault"]));
        }
    }
}