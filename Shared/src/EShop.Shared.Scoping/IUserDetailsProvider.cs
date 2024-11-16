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
    void ClearSystemUserContext();
    bool IsCurrentUser(string userId);
    string GetRawAccessToken();
}

public class UserData : ValueObject
{
    public const string SystemUsername = "System";
    public const string ContemiSupportGroup = "Contemi_Support";

    public UserData(string id, string username)
        : this(id, username, false, id)
    {
    }

    public UserData(
        string id,
        string username,
        bool isSupportUser,
        string? actionUserId = null,
        string userType = UserTypes.TenantUsers,
        string? actionUserType = null)
    {
        Id = id.ToLower();
        Username = username.ToLower();
        IsSupportUser = isSupportUser;
        ActionUserId = actionUserId?.ToLower() ?? this.Id;
        UserType = userType;
        ActionUserType = actionUserType ?? this.UserType;
    }

    public string Id { get; }
    public string Username { get; }
    public bool IsSupportUser { get; }
    public string ActionUserId { get; }
    public string UserType { get; }
    public string ActionUserType { get; }

    public static UserData GetSystemUser(string actionUserId, string? actionUserType = null)
        => new UserData(SystemUsername, SystemUsername, false, actionUserId, UserTypes.SystemUsers, actionUserType: actionUserType);

    public static bool IsSystemUser(string username) => username.Equals(SystemUsername, StringComparison.OrdinalIgnoreCase);
}

public static class UserTypes
{
    public const string TenantUsers = "TenantUsers";
    public const string SystemUsers = "SystemUsers";
    public const string AppClientWithoutIndividualUsers = "AppClientWithoutIndividualUsers";
    public const string AppClientWithIndividualUsers = "AppClientWithIndividualUsers";
}