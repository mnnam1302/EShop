using System.Security.Claims;

namespace EShop.Shared.Scoping.ResourceAccessControl;

/// <summary>
/// Defines cross-service claim type constants used throughout the EShop platform.
/// Contains both standard and custom claims for multi-tenant authentication and authorization.
/// </summary>
public static class EShopClaimTypes
{
    /// <summary>
    /// Standard user identifier claim. Maps to ClaimTypes.NameIdentifier.
    /// </summary>
    public const string UserId = ClaimTypes.NameIdentifier;

    /// <summary>
    /// Standard display name claim. Maps to ClaimTypes.Name.
    /// </summary>
    public const string DisplayName = ClaimTypes.Name;

    /// <summary>
    /// Username claim for authentication purposes.
    /// Used in JWT tokens and HTTP headers.
    /// </summary>
    public const string Username = "username";

    /// <summary>
    /// Tenant identifier claim for multi-tenant isolation.
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// Tenant groups claim for cross-tenant access control.
    /// Contains array of tenant IDs the user has access to.
    /// Format: ["tenant1", "tenant2", "eshop-support"]
    /// </summary>
    public const string TenantGroups = "tenant:groups";

    /// <summary>
    /// User type classification claim (e.g., "user", "admin", "system").
    /// </summary>
    public const string UserType = "user_type";

    // Authorization service specific claims
    /// <summary>
    /// RSA key identifier used for JWT token signing/validation.
    /// </summary>
    public const string KeyId = "key_id";

    /// <summary>
    /// Token version for future compatibility and migration scenarios.
    /// </summary>
    public const string TokenVersion = "token_version";
}