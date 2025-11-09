using EShop.Authorization.Tests.Setups;
using EShop.Shared.Authentication;
using Reqnroll;

namespace EShop.Authorization.Tests.Authorization;

[Binding]
internal sealed class Steps(ApiContext apiContext)
{
    [Then("the following users are set up")]
    public void ThenTheFollowingUsersAreSetUp(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows)
        {
            string username = row["Username"];
            string tenantId = row["TenantId"];

            var user = UserData.IsSystemUser(username)
                ? UserData.GetSystemUser(tenantId)
                : new UserData(username, username, tenantId);

            apiContext.AddUser(user);
        }
    }

    [Then("User {string} logs in")]
    public void ThenUserLogsIn(string username)
    {
        apiContext.SignIn(username);
    }

    [Then("User {string} has following permissions")]
    public void ThenUserHasFollowingPermissions(string username, DataTable dataTable)
    {
        var permissions = dataTable.Rows
            .Select(row => row["PermissionId"])
            .ToArray();

        apiContext.SetupPermissionsForUser(username, permissions);
    }

    [Given("System user with following permissions")]
    public void GivenSystemUserWithFollowingPermissions(DataTable dataTable)
    {
        var permissionIds = dataTable.Rows.Select(row => row["PermissionId"]).ToArray();
        apiContext.SetupPermissionsForDefaultAdminUser(permissionIds);
    }

    [Given("all features are available for System User")]
    public void GivenAllFeaturesAreAvailableForSystemUser()
    {
        apiContext.SetupStandardFeaturesForDefaultTenant();
    }
}
