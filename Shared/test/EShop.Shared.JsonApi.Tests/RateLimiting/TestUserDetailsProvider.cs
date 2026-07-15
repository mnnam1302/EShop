using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;

namespace EShop.Shared.JsonApi.Tests.RateLimiting;

// Reads tenant/user identity from plain test headers instead of a validated JWT, so these tests can
// exercise the real rate-limiting pipeline without standing up JWT signing/issuer infrastructure.
internal sealed class TestUserDetailsProvider(IHttpContextAccessor httpContextAccessor) : IUserDetailsProvider
{
    public const string TenantIdHeader = "X-Test-TenantId";
    public const string UserIdHeader = "X-Test-UserId";

    public bool IsAuthenticatedUser => GetHeader(TenantIdHeader) is not null;

    public bool IsSystemUser => false;

    public UserData AuthenticatedUser
    {
        get
        {
            var tenantId = GetHeader(TenantIdHeader) ?? throw new InvalidOperationException("No test tenant header set on the request.");
            var userId = GetHeader(UserIdHeader) ?? "test-user";
            return new UserData(userId, userId, tenantId);
        }
    }

    public void SetSystemUserContext(string onBehalfOfTenantId, string? onBehalfOfUserId = null, string? onBehalfOfUserType = null) =>
        throw new NotSupportedException();

    public void SetSystemUserContextWithEmptyScope() => throw new NotSupportedException();

    public void ClearSystemUserContext() => throw new NotSupportedException();

    public IDisposable CreateSystemUserScope(string? tenantId, string? userId = null, string? userType = null) =>
        throw new NotSupportedException();

    public bool IsCurrentUser(string userId) => throw new NotSupportedException();

    public string GetRawAccessToken() => throw new NotSupportedException();

    private string? GetHeader(string name)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        return httpContext.Request.Headers.TryGetValue(name, out var value) ? value.ToString() : null;
    }
}
