using EShop.Catalog.Tests.Setup;
using Reqnroll;

namespace EShop.Catalog.Tests.Shared;

[Binding]
public sealed class Steps(ApiContext apiContext)
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