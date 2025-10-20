using EShop.Authorization.Domain.Entities;
using EShop.Shared.Authentication;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using System.Security.Claims;

namespace EShop.Authorization.Application.Services;

public interface IMultiTenantClaimsBuilder
{
    /// <summary>
    /// Builds standard JWT claims for a tenant user including both tenant_id and tenant:groups.
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <returns>List of claims for JWT token</returns>
    Task<List<Claim>> BuildUserClaimsAsync(User user);

    /// <summary>
    /// Builds system user claims for internal service-to-service communication.
    /// </summary>
    /// <param name="tenantId">Target tenant context</param>
    /// <param name="actionUserId">Optional user being acted on behalf of</param>
    /// <returns>List of claims for system JWT token</returns>
    List<Claim> BuildSystemUserClaims(string tenantId);
}

public sealed class MultiTenantClaimsBuilder : IMultiTenantClaimsBuilder
{
    // In a real implementation, you might inject services to:
    // - Fetch user's organization memberships
    // - Check support user privileges
    // - Retrieve cross-tenant access permissions

    public async Task<List<Claim>> BuildUserClaimsAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(EShopClaimTypes.UserId, user.Id),
            new(EShopClaimTypes.Username, user.Username),
            new(EShopClaimTypes.DisplayName, user.Name),
            new(EShopClaimTypes.TenantId, user.TenantId),
            new(EShopClaimTypes.UserType, UserTypes.TenantUsers),
        };

        var accessibleTenants = await GetAccessibleTenantsAsync(user);
        foreach (var tenantId in accessibleTenants)
        {
            claims.Add(new Claim(EShopClaimTypes.TenantGroups, tenantId));
        }

        if (!string.IsNullOrEmpty(user.OrganizationId))
        {
            claims.Add(new Claim(EShopClaimTypes.OrganizationId, user.OrganizationId));
        }

        return claims;
    }

    public List<Claim> BuildSystemUserClaims(string tenantId)
    {
        var claims = new List<Claim>
        {
            new(EShopClaimTypes.UserId, UserData.SystemUsername),
            new(EShopClaimTypes.Username, UserData.SystemUsername),
            new(EShopClaimTypes.DisplayName, "System User"),
            new(EShopClaimTypes.TenantId, tenantId),
            new(EShopClaimTypes.UserType, UserTypes.SystemUsers),
            new(EShopClaimTypes.TenantGroups, tenantId), // System user has access to specified tenant
        };

        return claims;
    }

    /// <summary>
    /// Determines which tenants the user has access to.
    /// Primary tenant is always included as the first element.
    /// </summary>
    private async Task<List<string>> GetAccessibleTenantsAsync(User user)
    {
        var accessibleTenants = new List<string>
        {
            user.TenantId // Primary tenant is always first
        };

        // TODO: Implement business logic for cross-tenant access
        // Examples:
        // 1. Support users might have access to multiple tenants
        // 2. Users might be granted access to partner tenants
        // 3. Enterprise customers might have multiple tenant access
        // 4. Parent-child tenant relationships

        // For now, check if user is a support user
        if (await IsSupportUserAsync(user))
        {
            accessibleTenants.Add(UserData.EShopSupportGroup);
        }

        // Add any additional tenant access based on business rules
        var additionalTenants = await GetAdditionalTenantAccessAsync(user);
        accessibleTenants.AddRange(additionalTenants);

        return accessibleTenants.Distinct().ToList();
    }

    private async Task<bool> IsSupportUserAsync(User user)
    {
        // TODO: Implement logic to check if user is a support user
        // This might involve:
        // - Checking user roles
        // - Checking organization membership
        // - Checking specific permissions
        // - Database lookup for support user designation

        await Task.CompletedTask; // Placeholder for async operation
        return false; // Default: regular tenant user
    }

    private async Task<List<string>> GetAdditionalTenantAccessAsync(User user)
    {
        // TODO: Implement logic to fetch additional tenant access
        // This might involve:
        // - Cross-tenant permissions table
        // - Partner tenant relationships
        // - Enterprise multi-tenant access
        // - Temporary access grants

        await Task.CompletedTask; // Placeholder for async operation
        return new List<string>(); // Default: no additional access
    }
}