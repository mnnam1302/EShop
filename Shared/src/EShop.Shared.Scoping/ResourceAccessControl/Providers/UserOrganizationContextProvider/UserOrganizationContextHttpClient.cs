using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.DomainTools.Exceptions;
using Newtonsoft.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public sealed class UserOrganizationContextHttpClient
{
    private readonly HttpClient httpClient;
    private readonly IUserDetailsProvider userDetailsProvider;
    private readonly ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory;

    public UserOrganizationContextHttpClient(
        HttpClient httpClient,
        IUserDetailsProvider userDetailsProvider,
        ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory)
    {
        this.httpClient = httpClient;
        this.userDetailsProvider = userDetailsProvider;
        this.systemInternalJwtTokenFactory = systemInternalJwtTokenFactory;
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, CancellationToken cancellationToken)
    {
        if (!userDetailsProvider.IsAuthenticatedUser)
        {
            return new UserOrganizationContext();
        }

        var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(httpClient, userDetailsProvider.AuthenticatedUser, cancellationToken);
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
        var tenantId = userDetailsProvider.AuthenticatedUser.TenantId;
        var operationalUser = new UserData(userId, userId, tenantId, false, null, userType);

        var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(httpClient, operationalUser, cancellationToken);
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
        var operationalUser = userDetailsProvider.AuthenticatedUser;
        var systemUser = UserData.GetSystemUser(operationalUser.TenantId, operationalUser.Id);

        var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(httpClient, systemUser, cancellationToken);
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
        var operationalUser = userDetailsProvider.AuthenticatedUser;
        var systemUser = UserData.GetSystemUser(operationalUser.TenantId, operationalUser.Id);

        var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(httpClient, systemUser, cancellationToken);
        var response = await authenticatedClient.GetStringAsync($"api/v1/organizations?organizationContextPath={organizationContextPath}", cancellationToken);

        var result = JsonConvert.DeserializeObject<Result<OrganizationContext>>(response);
        if (result == null)
        {
            throw new NotFoundException($"Organization context with path '{organizationContextPath}' is not found.");
        }

        return result.Value;
    }
}
