using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EShop.Shared.Scoping.ResourceAccessControl;

public class HttpRequestUserDataProvider : IUserDetailsProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpRequestUserDataProvider> _logger;

    public const string TenantIdCustomHeaderName = "eshop-tenant-id";

    /// <summary>
    /// This is either <see cref="UserTypes.AppClientWithoutIndividualUsers"/> or <see cref="UserTypes.AppClientWithIndividualUsers"/>.
    /// Does not exist for tenant users.
    /// </summary>
    public const string UserTypeCustomHeaderName = "eshop-user-type";

    /// <summary>
    /// This is actually an application Id. Does not exist for tenant users.
    /// Does not exist for tenant users.
    /// </summary>
    public const string UserIdCustomHeaderName = "eshop-user-id";

    /// <summary>
    /// This is either an application name for <see cref="UserTypes.AppClientWithoutIndividualUsers"/>,
    /// or a username (login name) for <see cref="UserTypes.AppClientWithIndividualUsers"/>.
    /// Does not exist for tenant users.
    /// </summary>
    public const string ActionUserIdCustomHeaderName = "eshop-action-user-id";

    private Lazy<UserData> currentUser;
    private IDisposable? userLogContext;
    private IDisposable? tenantLogContext;

    public HttpRequestUserDataProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpRequestUserDataProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        // Read Lazy<T>: https://learn.microsoft.com/en-us/dotnet/framework/performance/lazy-initialization
        this.currentUser = new Lazy<UserData>(() => CreateUserFromRequest(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
    }

    public UserData AuthenticatedUser => this.currentUser.Value;

    public bool IsAuthenticatedUser => this.IsSystemUser || this.TryReadUserDataFromRequest(out _);

    public bool IsSystemUser
    {
        get
        {
            UserData? userData;
            if (this.currentUser.IsValueCreated)
            {
                userData = this.currentUser.Value;
            }
            else if (this.TryReadUserDataFromRequest(out var userDataFromToken))
            {
                userData = userDataFromToken;
            }
            else
            {
                return false;
            }

            if (userData == null)
            {
                return false;
            }

            return userData.Id.Equals(UserData.SystemUsername, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void SetSystemUserContextWithEmptyScope()
    {
        _logger.LogDebug("Setting user context to System with no tenant scope [{providerHash}]", this.GetHashCode());
        this.currentUser = new Lazy<UserData>(() => UserData.GetSystemUser(null), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        var updatedToUser = this.currentUser.Value ?? throw new InvalidOperationException("User should have been set");
        _logger.LogTrace("User context set to '{userId}'('{tenantId}').", updatedToUser.Id, updatedToUser.TenantId);
    }

    public void SetSystemUserContext(string onBehalfOfTenantId, string? onBehalfOfUserId = null, string? onBehalfOfUserType = null)
    {
        if (string.IsNullOrWhiteSpace(onBehalfOfTenantId))
        {
            throw new ArgumentException($"'{nameof(onBehalfOfTenantId)}' cannot be null or whitespace", nameof(onBehalfOfTenantId));
        }

        _logger.LogDebug("Setting user context to System (Tenant ID='{tenantId}', User Id='{userId}') [{providerHash}].", onBehalfOfTenantId, onBehalfOfUserId, this.GetHashCode());
        this.currentUser =
            string.IsNullOrWhiteSpace(onBehalfOfUserId)
            ? new Lazy<UserData>(() => UserData.GetSystemUser(onBehalfOfTenantId), System.Threading.LazyThreadSafetyMode.PublicationOnly)
            : new Lazy<UserData>(() => UserData.GetSystemUser(onBehalfOfTenantId, onBehalfOfUserId, onBehalfOfUserType), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        var updatedToUser = this.currentUser.Value ?? throw new InvalidOperationException("User should have been set");
        _logger.LogTrace("User context set to '{userId}'('{tenantId}').", updatedToUser.Id, updatedToUser.TenantId);
    }

    public void ClearSystemUserContext()
    {
        _logger.LogDebug("Resetting user context [{providerHash}].", this.GetHashCode());
        this.currentUser = new Lazy<UserData>(() => CreateUserFromRequest(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        this.userLogContext?.Dispose();
        this.tenantLogContext?.Dispose();
    }

    public string GetRawAccessToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
    }

    public bool IsCurrentUser(string userId)
    {
        return string.Equals(userId, this.currentUser.Value.Id, StringComparison.OrdinalIgnoreCase);
    }

    private UserData CreateUserFromRequest()
    {
        if (!TryReadUserDataFromRequest(out var user))
        {
            throw new InvalidRequestException((int)System.Net.HttpStatusCode.Unauthorized, "Invalid request");
        }

        return user!;
    }

    private bool TryReadUserDataFromRequest(out UserData? user)
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request == null)
        {
            _logger.LogTrace("Found empty request");
            user = null;
            return false;
        }

        var userType = request.Headers[UserTypeCustomHeaderName].FirstOrDefault();

        if (UserTypes.TenantUsers.Equals(userType, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetUserFromHeaders(request, UserTypes.TenantUsers, out user);
        }

        if (UserTypes.SystemUsers.Equals(userType, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetUserFromHeaders(request, UserTypes.SystemUsers, out user);
        }

        if (UserTypes.AppClientWithoutIndividualUsers.Equals(userType, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetUserFromHeaders(request, UserTypes.AppClientWithoutIndividualUsers, out user);
        }

        if (UserTypes.AppClientWithIndividualUsers.Equals(userType, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetUserFromHeaders(request, UserTypes.AppClientWithIndividualUsers, out user);
        }

        return TryGetTenantUserFromAccessToken(out user);
    }

    private bool TryGetUserFromHeaders(HttpRequest request, string userType, out UserData? user)
    {
        var userId = request.Headers[UserIdCustomHeaderName].FirstOrDefault();
        var tenantId = request.Headers[TenantIdCustomHeaderName].FirstOrDefault();
        var actionUserId = request.Headers[ActionUserIdCustomHeaderName].FirstOrDefault();

        try
        {
            if (UserData.SystemUsername.Equals(userId, StringComparison.OrdinalIgnoreCase))
            {
                user = actionUserId is null
                    ? UserData.GetSystemUser(tenantId)
                    : UserData.GetSystemUser(tenantId, actionUserId, actionUserType: userType);
                return true;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty.");
            }

            user = new UserData(userId, userId, tenantId, false, actionUserId, userType, actionUserType: userType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Couldn't parse user from custom HTTP headers");
            user = null;
            return false;
        }
    }

    private bool TryGetTenantUserFromAccessToken(out UserData? user)
    {
        var accessToken = ReadAccessToken();

        if (accessToken == null)
        {
            _logger.LogTrace("Found empty access token");
            user = null;
            return false;
        }

        try
        {
            // We are using username as id for users stored in our local database (as it is unique across tenants anyway)
            var username = accessToken.Claims.First(x => x.Type == "username").Value;
            var tenantGroups = accessToken.Claims.Where(x => x.Type == "tenant:groups").Select(x => x.Value).ToList();

            var defaultTenantGroup = tenantGroups.Count > 1
                ? tenantGroups.First(x => !x.Equals(UserData.EShopSupportGroup, StringComparison.OrdinalIgnoreCase))
                : tenantGroups.FirstOrDefault();

            user = new UserData(
                username,
                username,
                defaultTenantGroup ?? string.Empty,
                tenantGroups.Contains(UserData.EShopSupportGroup),
                username);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Couldn't parse user from access token");
            user = null;
            return false;
        }
    }

    private JsonWebToken? ReadAccessToken()
    {
        var rawAccessToken = GetRawAccessToken();
        var accessTokenEncoded = JwtEncodedStringHelper.GetJwtEncodedString(rawAccessToken);

        if (string.IsNullOrWhiteSpace(accessTokenEncoded))
        {
            return null;
        }

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.ReadJsonWebToken(accessTokenEncoded); // Read JsonWebToken: not validation

        if (token == null)
        {
            return null;
        }

        var expirationClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expirationClaim != null && long.TryParse(expirationClaim.Value, out var exp))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            if (expirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Access token has expired");
                return null;
            }
        }

        return token;
    }
}