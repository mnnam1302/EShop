using EShop.Identity.Domain.Entities;
using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.Scoping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Tests.Steps.StepContext;

internal class UserContext
{
    private const string AuthCollectionUri = "api/v1/auth/register";
    private const string UsersCollectionUrl = "api/v1/users";
    private const string RolesCollectionUrl = "api/v1/roles";
    private const string PermissionsCollectionUrl = "api/v1/permissions";

    private readonly ApiContext _apiContext;
    private readonly ILogger<UserContext> _logger;

    public UserContext(ApiContext apiContext)
    {
        _apiContext = apiContext;
        _logger = apiContext.ServiceProvider.GetRequiredService<ILogger<UserContext>>();
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
    public string UserGroup { get; internal set; }

    internal async Task SimulateTenantUserCreationAsync(
        string tenantId,
        string username,
        string displayName,
        string email,
        string group,
        bool setAsDefault = true)
    {
        try
        {
            var command = new Command.RegisterUser(username, "password", email, displayName)
            {
                //OrganizationId = tenantId
            };
            var result = await _apiContext.Post<Command.RegisterUser>(AuthCollectionUri, command);

            _apiContext.AddUser(
                new UserData(username, username, tenantId, group == UserData.EShopSupportGroup),
                setAsDefault);

            _apiContext.GrantAllPermissionsToUser(username);
        }
        catch(Exception ex)
        {
            _logger.LogWarning("User error");
            this.Error = ex;
        }
    }
}