using System.Security.Claims;

namespace EShop.Shared.Scoping.ResourceAccessControl;

/// <summary>
/// Defines cross-service claim type constants used throughout the EShop platform.
/// Contains both standard and custom claims for multi-tenant authentication and authorization.
/// </summary>
public static class EShopClaimTypes
{
    /// <summary>
    /// Standard JWT subject claim. 
    /// Use this instead of ClaimTypes.NameIdentifier to avoid JWT serialization conversion issues.
    /// </summary>
    public const string UserId = "sub";

    /// <summary>
    /// Username claim for authentication purposes.
    /// Used in JWT tokens and HTTP headers.
    /// </summary>
    public const string Username = "username";

    /// <summary>
    /// Standard display name claim. Maps to ClaimTypes.Name.
    /// </summary>
    public const string DisplayName = ClaimTypes.Name;

    /// <summary>
    /// Primary tenant identifier claim for multi-tenant isolation.
    /// Represents the user's HOME tenant - the primary tenant they belong to.
    /// Single value: "tenant-abc123"
    /// Usage: Data scoping, primary tenant operations, billing context.
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// Tenant groups claim for cross-tenant access control.
    /// Contains array of ALL tenant IDs the user has access to (including primary tenant).
    /// Multiple values: ["tenant-abc123", "tenant-xyz789", "eshop-support"]
    /// Usage: Cross-tenant operations, support access, tenant switching, multi-tenant scenarios.
    /// Always includes the primary tenant_id as the first element.
    /// </summary>
    public const string TenantGroups = "tenant:groups";

    /// <summary>
    /// User type classification claim (e.g., "TenantUsers", "SystemUsers", "SupportUsers").
    /// </summary>
    public const string UserType = "user_type";

    /// <summary>
    /// Organization identifier claim for organizational context within a tenant.
    /// </summary>
    public const string OrganizationId = "organization_id";

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