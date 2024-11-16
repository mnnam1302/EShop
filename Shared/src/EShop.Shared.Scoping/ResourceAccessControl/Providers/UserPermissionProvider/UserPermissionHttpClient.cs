using Newtonsoft.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public class UserPermissionHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UserPermissionHttpClient(HttpClient httpClient, IUserDetailsProvider userDetailsProvider)
    {
        _httpClient = httpClient;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<string[]> GetPermissionsForCurrentUser()
    {
        //var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(client, userDetailsProvider.AuthenticatedUser);
        //var response = await authenticatedClient.GetStringAsync("/api/v1/userPermissions");
        var response = await _httpClient.GetStringAsync("/api/v1/userPermissions");

        var permissions = JsonConvert.DeserializeObject<string[]>(response);
        return permissions;
    }
}