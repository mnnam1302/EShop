using EShop.Shared.Contracts.Abstractions.Shared;
using Newtonsoft.Json;
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

        var response = await authenticatedClient.GetStringAsync("api/v1/userOrganizationContext");
        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);

        return result?.Value ?? throw new UserOrganizationContextNotFoundException();
    }

    public async Task<UserOrganizationContext> GetUserOrganizationContextAsync(string userId, string userType = UserTypes.TenantUsers)
    {
        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;
        //var oAuthScopes = _userDetailsProvider.AuthenticatedUser.OAuthScopes;

        var userData = new UserData(userId, userId, tenantId, false, null, userType);

        //var authenticatedHttpRequestMessage = HttpRequestUserContextFactory.AddUserContext(httpRequestMessage, userData);
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync("api/v1/userOrganizationContext");
        var result = JsonConvert.DeserializeObject<Result<UserOrganizationContext>>(response);

        return result?.Value ?? throw new UserOrganizationContextNotFoundException();
    }
}