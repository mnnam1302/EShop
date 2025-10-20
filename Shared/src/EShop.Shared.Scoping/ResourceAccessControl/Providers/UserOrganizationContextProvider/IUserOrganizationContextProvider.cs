using EShop.Shared.Authentication;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public interface IUserOrganizationContextProvider
{
    Task<UserOrganizationContext> GetUserOrganizationContextAsync(CancellationToken cancellationToken = default);

    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string userType = UserTypes.TenantUsers, CancellationToken cancellationToken = default);

    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId, CancellationToken cancellationToken = default);

    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath, CancellationToken cancellationToken = default);
}