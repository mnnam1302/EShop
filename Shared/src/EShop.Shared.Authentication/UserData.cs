using EShop.Shared.DomainTools.ValueObjects;

namespace EShop.Shared.Authentication;

public sealed class UserData : ValueObject
{
    public const string SystemUsername = "system";
    public const string SystemTenantId = "system";
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
        ActionUserId = actionUserId?.ToLower() ?? Id;
        UserType = userType;
        ActionUserType = actionUserType ?? UserType;
    }

    public string Id { get; }
    public string Username { get; }
    public string TenantId { get; }
    public bool IsSupportUser { get; }
    public string ActionUserId { get; }
    public string UserType { get; }
    public string ActionUserType { get; }

    public static UserData GetSystemUser(string? tenantId)
    {
        return new UserData(SystemUsername, SystemUsername, tenantId ?? string.Empty);
    }

    public static UserData GetSystemUser(string? tenantId, string actionUserId, string? actionUserType = null)
    {
        return new UserData(
            SystemUsername,
            SystemUsername,
            tenantId ?? string.Empty,
            false,
            actionUserId,
            UserTypes.SystemUsers,
            actionUserType: actionUserType);
    }

    public static bool IsSystemUser(string username) => username.Equals(SystemUsername, StringComparison.OrdinalIgnoreCase);

    public bool CanCreateTenant() => IsSystemUser(Username) || (IsSupportUser && UserType == UserTypes.SystemUsers);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Id;
        yield return Username;
        yield return TenantId;
        yield return IsSupportUser;
        yield return ActionUserId;
        yield return UserType;
        yield return ActionUserType;
    }
}
