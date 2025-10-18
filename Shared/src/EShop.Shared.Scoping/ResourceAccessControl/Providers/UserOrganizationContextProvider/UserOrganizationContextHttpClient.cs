using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Scoping.Exceptions;
using Newtonsoft.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public sealed class UserOrganizationContextHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UserOrganizationContextHttpClient(HttpClient httpClient, IUserDetailsProvider userDetailsProvider)
    {
        _httpClient = httpClient;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, CancellationToken cancellationToken)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return new UserOrganizationContext();
        }

        // TODO: Please Kodi strongly focus here since inconsistency with JwtTokenManager (JwtToken & SecretKey) Authorization service
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, _userDetailsProvider.AuthenticatedUser);
        var response = await authenticatedClient.GetStringAsync($"api/v1/users/{userId}/organizationContext", cancellationToken);

        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);
        if (result == null)
        {
            throw new NotFoundException($"User organization context '{userId}' is not found.");
        }

        return result.Value;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, string userType = UserTypes.TenantUsers, CancellationToken cancellationToken = default)
    {
        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;
        var operationalUser = new UserData(userId, userId, tenantId, false, null, userType);

        // TODO: Please Kodi strongly focus here since inconsistency with JwtTokenManager (JwtToken & SecretKey) Authorization service
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, operationalUser);
        var response = await authenticatedClient.GetStringAsync($"api/v1/users/{operationalUser.Id}/organizationContext", cancellationToken);

        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);
        if (result == null)
        {
            throw new NotFoundException($"User organization context '{userId}' is not found.");
        }

        return result.Value;
    }

    public async Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken)
    {
        var operationalUser = _userDetailsProvider.AuthenticatedUser;
        var systemUser = UserData.GetSystemUser(operationalUser.TenantId, operationalUser.Id);

        // TODO: Please Kodi strongly focus here since inconsistency with JwtTokenManager (JwtToken & SecretKey) Authorization service
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, systemUser);
        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations/{organizationId}/organizationContext", cancellationToken);

        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);
        if (result == null)
        {
            throw new NotFoundException($"Organization context with Id '{organizationId}' is not found.");
        }

        return result.Value;
    }

    public async Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken)
    {
        var operationalUser = _userDetailsProvider.AuthenticatedUser;
        var systemUser = UserData.GetSystemUser(operationalUser.TenantId, operationalUser.Id);

        // TODO: Please Kodi strongly focus here since inconsistency with JwtTokenManager (JwtToken & SecretKey) Authorization service
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, systemUser);
        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations?organizationContextPath={organizationContextPath}", cancellationToken);

        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);
        if (result == null)
        {
            throw new NotFoundException($"Organization context with path '{organizationContextPath}' is not found.");
        }

        return result.Value;
    }
}