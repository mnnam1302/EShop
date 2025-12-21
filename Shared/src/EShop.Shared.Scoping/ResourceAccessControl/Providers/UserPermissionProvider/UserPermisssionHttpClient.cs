using EShop.Shared.Authentication.Abstractions;
using System.Net.Http.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public sealed class UserPermisssionHttpClient(
    HttpClient httpClient,
    IUserDetailsProvider userDetailsProvider,
    ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory)
{
    public async Task<string[]> GetPermissionsForCurrentUser()
    {
        try
        {
            var userData = userDetailsProvider.AuthenticatedUser;
            var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(httpClient, userData);

            var response = await authenticatedClient.GetAsync($"users/{userData.Id}/permissions");

            response.EnsureSuccessStatusCode();

            var permissions = await response.Content.ReadFromJsonAsync<string[]>();

            return permissions ?? [];
        }
        catch (Exception ex)
        {
            // Log the exception
            return [];
        }
    }
}