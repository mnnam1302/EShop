using EventFlow.ValueObjects;

namespace EShop.Shared.Scoping;

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
    public const string SystemUsername = "System";
    public const string ContemiSupportGroup = "Contemi_Support";

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
        string? oAuthScopes = null,
        string? actionUserType = null)
    {
        Id = id.ToLower();
        Username = username.ToLower();
        TenantId = tenantId;
        IsSupportUser = isSupportUser;
        ActionUserId = actionUserId?.ToLower() ?? this.Id;
        UserType = userType;
        OAuthScopes = oAuthScopes;
        ActionUserType = actionUserType ?? this.UserType;
    }

    public string Id { get; }
    public string Username { get; }
    public string TenantId { get; }
    public bool IsSupportUser { get; }
    public string ActionUserId { get; }
    public string UserType { get; }
    public string? OAuthScopes { get; }
    public string ActionUserType { get; }

    public static UserData GetSystemUser(string? tenantId)
        => new UserData(
            SystemUsername,
            SystemUsername,
            tenantId ?? string.Empty);

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
    public const string TenantUsers = "TenantUsers";
    public const string SystemUsers = "SystemUsers";
    public const string AppClientWithoutIndividualUsers = "AppClientWithoutIndividualUsers";
    public const string AppClientWithIndividualUsers = "AppClientWithIndividualUsers";
}