namespace EShop.Shared.Authentication;

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