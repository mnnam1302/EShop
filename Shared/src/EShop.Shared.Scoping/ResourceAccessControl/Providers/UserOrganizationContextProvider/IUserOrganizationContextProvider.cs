using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public interface IUserOrganizationContextProvider
{
    Task<UserOrganizationContext> GetUserOrganizationContextAsync();

    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers);

    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId);

    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath);
}