using EShop.Shared.Authentication;
using EShop.Tenancy.Tests.Setups;
using Reqnroll;

namespace EShop.Tenancy.Tests.Shared
{
    [Binding]
    internal class Steps(ApiContext apiContext)
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

        [Given("System User has logged in")]
        public void GivenSystemUserHasLoggedIn()
        {
            apiContext.UserLogsIn(UserData.SystemUsername);
        }
    }
}
