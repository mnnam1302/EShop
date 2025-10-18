using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Scoping.Exceptions;
using Newtonsoft.Json;

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

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return new UserOrganizationContext();
        }

        // TODO: Please Kodi strongly focus here since inconsistency with JwtTokenManager (JwtToken & SecretKey) Authorization service
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, _userDetailsProvider.AuthenticatedUser);

        var response = await authenticatedClient.GetStringAsync($"api/v1/users/{userId}/organizationContext", cancellationToken);
        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);

        return result?.Value ?? throw new NotFoundException($"User organization context '{userId}' is not found.");
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, string userType = UserTypes.TenantUsers, CancellationToken cancellationToken = default)
    {
        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;
        var operationalUser = new UserData(userId, userId, tenantId, false, null, userType);

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, operationalUser);

        var response = await authenticatedClient.GetStringAsync($"api/v1/users/{operationalUser.Id}/organizationContext", cancellationToken);
        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);

        return result?.Value ?? throw new NotFoundException($"User organization context '{operationalUser.Id}' is not found.");
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
    {
        var userData = UserData.GetSystemUser(
            _userDetailsProvider.AuthenticatedUser.TenantId,
            _userDetailsProvider.AuthenticatedUser.Id);

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations/{organizationId}");
        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);

        return result?.Value ?? throw new NotFoundException("Organization context is not found.");
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
    {
        var userData = UserData.GetSystemUser(
           _userDetailsProvider.AuthenticatedUser.TenantId,
           _userDetailsProvider.AuthenticatedUser.Id);

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations?organizationContextPath={organizationContextPath}");
        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);

        return result?.Value ?? throw new NotFoundException("Organization context is not found.");
    }
}