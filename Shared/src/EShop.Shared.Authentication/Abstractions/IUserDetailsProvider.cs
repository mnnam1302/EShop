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

    /// <summary>
    /// Creates a disposable scope that sets system user context on creation and clears it on disposal.
    /// Use in background jobs, DB initializers, and service-internal scope changes.
    /// Pass null tenantId for empty scope (system-wide operations).
    /// </summary>
    IDisposable CreateSystemUserScope(string? tenantId, string? userId = null, string? userType = null);

    bool IsCurrentUser(string userId);

    string GetRawAccessToken();
}
