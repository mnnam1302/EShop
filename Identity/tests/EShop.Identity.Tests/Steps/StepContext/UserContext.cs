using EShop.Identity.Domain.Entities;
using EShop.Identity.Tests.Setups;

namespace EShop.Identity.Tests.Steps.StepContext;

internal class UserContext
{
    private const string UsersCollectionUrl = "api/v1/users";
    private const string RolesCollectionUrl = "api/v1/roles";
    private const string PermissionsCollectionUrl = "api/v1/permissions";

    private readonly ApiContext _apiContext;

    public UserContext(ApiContext apiContext)
    {
        _apiContext = apiContext;
    }

    public List<User> RetrievedUsers = new List<User>();
    public string DisplayName { get; internal set; }
    public string Email { get; internal set; }
    public string Username { get; internal set; }
    public string PhoneNumber { get; internal set; }
    public Exception Error { get; set; }
    public Role[] RolesOfUser { get; internal set; }
    public Permission[] Permissions { get; private set; }
    public string OrganizationName { get; internal set; }
    public Organization RetrievedUserOrganization { get; private set; }
    public string LanguageCode { get; internal set; }
    public string UserGroup { get; internal set; }
}