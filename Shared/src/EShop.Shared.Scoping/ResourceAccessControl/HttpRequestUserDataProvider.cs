using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EShop.Shared.Scoping.ResourceAccessControl;

public class HttpRequestUserDataProvider : IUserDetailsProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpRequestUserDataProvider> _logger;

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

    public HttpRequestUserDataProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpRequestUserDataProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

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

    public void ClearSystemUserContext()
    {
        _logger.LogDebug("Resetting user context [{providerHash}].", this.GetHashCode());
        this.currentUser = new Lazy<UserData>(() => CreateUserFromRequest(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        this.userLogContext?.Dispose();
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
            throw new InvalidRequestException((int)System.Net.HttpStatusCode.Unauthorized);
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

        return TryGetUserFromAccessToken(out user);
    }

    private bool TryGetUserFromHeaders(HttpRequest request, string userType, out UserData? user)
    {
        var userId = request.Headers[UserIdCustomHeaderName].FirstOrDefault();
        var actionUserId = request.Headers[ActionUserIdCustomHeaderName].FirstOrDefault();

        try
        {
            if (UserData.SystemUsername.Equals(userId, StringComparison.OrdinalIgnoreCase) && actionUserId is not null)
            {
                user = UserData.GetSystemUser(actionUserId, actionUserType: userType);
                return true;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            user = new UserData(userId, userId, false, actionUserId, userType, actionUserType: userType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Couldn't parse user from custom HTTP headers");
            user = null;
            return false;
        }
    }

    private bool TryGetUserFromAccessToken(out UserData? user)
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

            user = new UserData(username, username);

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

        return new JsonWebTokenHandler().ReadJsonWebToken(accessTokenEncoded);
    }
}