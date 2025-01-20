using EShop.Identity.Tests.Setups;
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
        // todo, when implement tenancy service
    }
}