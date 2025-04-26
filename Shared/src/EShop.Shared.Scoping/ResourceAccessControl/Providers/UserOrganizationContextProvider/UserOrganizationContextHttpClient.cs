using EShop.Shared.Contracts.Abstractions.Shared;
using Newtonsoft.Json;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public class UserOrganizationContextHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UserOrganizationContextHttpClient(HttpClient httpClient, IUserDetailsProvider userDetailsProvider)
    {
        _httpClient = httpClient;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync()
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return new UserOrganizationContext();
        }

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, _userDetailsProvider.AuthenticatedUser);

        var response = await authenticatedClient.GetStringAsync($"api/v1/users/{_userDetailsProvider.AuthenticatedUser.Id}");
        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);

        return result?.Value ?? throw new UserOrganizationContextNotFoundException();
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, string userType = UserTypes.TenantUsers)
    {
        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;

        var userData = new UserData(userId, userId, tenantId, false, null, userType);

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync($"api/v1/users/{userId}");
        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);

        return result?.Value ?? throw new UserOrganizationContextNotFoundException();
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
    {
        var userData = UserData.GetSystemUser(
            _userDetailsProvider.AuthenticatedUser.TenantId,
            _userDetailsProvider.AuthenticatedUser.Id);

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations/{organizationId}");
        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);

        return result?.Value ?? throw new OrganizationContextNotFoundException();
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
    {
        var userData = UserData.GetSystemUser(
           _userDetailsProvider.AuthenticatedUser.TenantId,
           _userDetailsProvider.AuthenticatedUser.Id);

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations?organizationContextPath={organizationContextPath}");
        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);

        return result?.Value ?? throw new OrganizationContextNotFoundException();
    }
}