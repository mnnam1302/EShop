namespace EShop.Shared.Authentication.Abstractions;

/// <summary>
/// This interface represents contain current user in HttpRequest. 
/// After authentication and verify access token, add some info user into header and pass to HttpRequest.
/// </summary>
public interface IUserDetailsProvider
{
    UserData AuthenticatedUser { get; }

    bool IsAuthenticatedUser { get; }

    bool IsSystemUser { get; }

    void SetSystemUserContext(string onBehalfOfTenantId, string? onBehalfOfUserId = null, string? onBehalfOfUserType = null);

    void SetSystemUserContextWithEmptyScope();

    void ClearSystemUserContext();

    bool IsCurrentUser(string userId);

    string GetRawAccessToken();
}
