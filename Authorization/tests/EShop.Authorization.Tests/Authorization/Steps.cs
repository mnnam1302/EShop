using EShop.Authorization.Tests.Setups;
using Reqnroll;

namespace EShop.Authorization.Tests.Authorization;

[Binding]
internal sealed class Steps(ApiContext apiContext)
{
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
