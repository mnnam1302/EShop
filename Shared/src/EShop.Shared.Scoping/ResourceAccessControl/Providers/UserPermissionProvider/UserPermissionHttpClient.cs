using Newtonsoft.Json;
using System.Net.Http.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public class UserPermissionHttpClient
{
    private const string userPermissionsEndpoint = "/api/v1/userPermissions";
    private readonly HttpClient _httpClient;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UserPermissionHttpClient(HttpClient httpClient, IUserDetailsProvider userDetailsProvider)
    {
        _httpClient = httpClient;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<string[]> GetPermissionsForCurrentUser()
    {
        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, _userDetailsProvider.AuthenticatedUser);

        var response = await authenticatedClient.GetStringAsync("/api/v1/userPermissions");

        var permissions = JsonConvert.DeserializeObject<string[]>(response);
        return permissions ?? Array.Empty<string>();
    }
}