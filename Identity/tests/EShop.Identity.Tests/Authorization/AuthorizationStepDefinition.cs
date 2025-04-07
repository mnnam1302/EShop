using EShop.Identity.Tests.Setups;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Reqnroll;

namespace EShop.Identity.Tests.Authorization;

[Binding]
public class AuthorizationStepDefinition
{
    private static readonly string[] allPermissionIds = [
        PermissionConstants.ViewOrganizationsPermissionId,
        PermissionConstants.ManageOrganizationsPermissionId
    ];

    private readonly ApiContext _apiContext;

    public AuthorizationStepDefinition(ApiContext apiContext)
    {
        _apiContext = apiContext;
    }

    [Given("Admin user with all permissions")]
    public void GivenAdminUserWithAllPermissions()
    {
        _apiContext.SetupPermissionsForDefaultAdminUser(allPermissionIds);
    }

    [Given("all standard features were turned on for test tenant")]
    public void GivenAllStandardFeaturesWereTurnedOnForTestTenant()
    {
        _apiContext.SetupStandardFeaturesForDefaultTenant();
    }

    [Given("all standard features were turned on for '(.*)'")]
    public void GivenAllStandardFeaturesWereTurnedOnFor(string tenantId)
    {
        _apiContext.SetupStandardFeaturesForTenant(tenantId);
    }

    [Given("user '(.*)' has the following permissions")]
    public void GivenUserHasTheFollowingPermissions(string username, Table dataTable)
    {
        var permissions = dataTable.Rows.Select(row => row["PermissionId"]).ToArray();
        _apiContext.SetupPermissionsForUser(username, permissions);
    }

    [Given("the following users are set up")]
    public void GivenTheFollowingUsersAreSetUp(Table userTable)
    {
        foreach (var row in userTable.Rows)
        {
            string username = row["Username"];
            string tenantId = row["TenantId"];

            var user = UserData.IsSystemUser(username)
                ? UserData.GetSystemUser(tenantId)
                : new UserData(username, username, tenantId);

            _apiContext.AddUser(user);
        }
    }
}