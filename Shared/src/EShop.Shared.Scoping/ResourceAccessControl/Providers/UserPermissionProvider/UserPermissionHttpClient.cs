using EShop.Shared.Authentication.Abstractions;
using Newtonsoft.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public sealed class UserPermissionHttpClient
{
    private const string userPermissionsEndpoint = "api/v1/users";

    private readonly HttpClient _httpClient;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ISystemInternalJwtTokenFactory _systemInternalJwtTokenFactory;

    public UserPermissionHttpClient(
        HttpClient httpClient,
        IUserDetailsProvider userDetailsProvider,
        ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory)
    {
        _httpClient = httpClient;
        _userDetailsProvider = userDetailsProvider;
        _systemInternalJwtTokenFactory = systemInternalJwtTokenFactory;
    }

    public async Task<string[]> GetPermissionsForCurrentUser()
    {
        var userData = _userDetailsProvider.AuthenticatedUser;
        var authenticatedClient = await _systemInternalJwtTokenFactory.AddUserContext(_httpClient, userData);

        var response = await authenticatedClient.GetStringAsync($"{userPermissionsEndpoint}/{userData.Id}/permissions");

        var permissions = JsonConvert.DeserializeObject<string[]>(response);
        return permissions ?? Array.Empty<string>();
    }
}