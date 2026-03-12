using EShop.Authorization.Tests.Setups;
using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using Reqnroll;

namespace EShop.Authorization.Tests.Authorization;

[Binding]
internal sealed class Steps(ApiContext apiContext)
{
    [Given("the following users are set up")]
    [Then("the following users are set up")]
    public void GivenTheFollowingUsersAreSetUp(DataTable dataTable)
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

    [Given("user {string} logs in to the system")]
    [Then("user {string} logs in to the system")]
    public void GivenUserLogsInToTheSystem(string username)
    {
        apiContext.SignIn(username);
    }

    [Given("user {string} has the following permissions")]
    [Then("user {string} has following permissions")]
    public void GivenUserHasTheFollowingPermissions(string username, DataTable dataTable)
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

    [Given("all standard features were turned on for {string}")]
    [Then("all standard features were turned on for {string}")]
    public void GivenAllStandardFeaturesWereTurnedOnFor(string tenantName)
    {
        apiContext.SetupStandardFeaturesForTenant(tenantName);
    }

    [Given("Tenancy service has provisioned a new tenant with following details")]
    public async Task GivenTenancyServiceHasProvisionedANewTenantWithFollowingDetails(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows)
        {
            var tenantId = row["TenantId"];
            var tenantName = row["TenantName"];
            var ownerUsername = row["OwnerUsername"];
            var ownerDisplayName = row["OwnerDisplayName"];
            var ownerEmail = row["OwnerEmail"];

            // Simulate the TenantCreated event that would trigger root organization creation
            await apiContext.PublishIntegrationEvent<ITenantCreated>(new
            {
                TenantId = tenantId,
                TenantName = tenantName,
                OwnerUsername = ownerUsername,
                OwnerDisplayName = ownerDisplayName,
                OwnerEmail = ownerEmail
            });
        }
    }
}