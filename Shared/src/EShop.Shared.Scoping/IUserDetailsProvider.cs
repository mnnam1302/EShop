using EventFlow.ValueObjects;

namespace EShop.Shared.Scoping;

/// <summary>
/// This interface represents contain current user in HttpRequest. After authentication and verify access token, add some info user into header and pass to HttpRequest.
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

public class UserData : ValueObject
{
    public const string SystemUsername = "system";
    public const string EShopSupportGroup = "eshop-support";

    public UserData(string id, string username, string tenantId)
        : this(id, username, tenantId, false, id)
    {
    }

    public UserData(
        string id,
        string username,
        string tenantId,
        bool isSupportUser,
        string? actionUserId = null,
        string userType = UserTypes.TenantUsers,
        string? actionUserType = null)
    {
        Id = id.ToLower();
        Username = username.ToLower();
        TenantId = tenantId;
        IsSupportUser = isSupportUser;
        ActionUserId = actionUserId?.ToLower() ?? this.Id;
        UserType = userType;
        ActionUserType = actionUserType ?? this.UserType;
    }

    public string Id { get; }
    public string Username { get; }
    public string TenantId { get; }
    public bool IsSupportUser { get; }
    public string ActionUserId { get; }
    public string UserType { get; }
    public string ActionUserType { get; }

    public static UserData GetSystemUser(string? tenantId)
        => new UserData(SystemUsername, SystemUsername, tenantId ?? string.Empty);

    public static UserData GetSystemUser(string? tenantId, string actionUserId, string? actionUserType = null)
        => new UserData(
            SystemUsername,
            SystemUsername,
            tenantId ?? string.Empty,
            false,
            actionUserId,
            UserTypes.SystemUsers,
            actionUserType: actionUserType);

    public static bool IsSystemUser(string username) => username.Equals(SystemUsername, StringComparison.OrdinalIgnoreCase);
}

public static class UserTypes
{
    public const string TenantUsers = nameof(TenantUsers);
    public const string SystemUsers = nameof(SystemUsers);
    public const string AppClientWithoutIndividualUsers = nameof(AppClientWithoutIndividualUsers);
    public const string AppClientWithIndividualUsers = nameof(AppClientWithIndividualUsers);

    private static readonly Dictionary<string, string> UserTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { TenantUsers, TenantUsers },
        { SystemUsers, SystemUsers },
        { AppClientWithoutIndividualUsers, AppClientWithoutIndividualUsers },
        { AppClientWithIndividualUsers, AppClientWithIndividualUsers }
    };

    public static bool IsTenantUser(string userType) => IsUserType(userType, TenantUsers);

    public static bool IsSystemUser(string userType) => IsUserType(userType, SystemUsers);

    public static bool IsAppClientWithoutIndividualUsers(string userType) => IsUserType(userType, AppClientWithoutIndividualUsers);

    public static bool IsAppClientWithIndividualUsers(string userType) => IsUserType(userType, AppClientWithIndividualUsers);

    public static bool IsUserType(string userType, string targetType)
    {
        return UserTypeMappings.TryGetValue(targetType, out var mappedType) && userType.Equals(mappedType, StringComparison.OrdinalIgnoreCase);
    }
}