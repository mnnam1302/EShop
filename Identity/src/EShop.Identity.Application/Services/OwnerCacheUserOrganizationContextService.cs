using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

namespace EShop.Identity.Application.Services;

public class OwnerCacheUserOrganizationContextService : IUserOrganizationContextProvider
{
    // DI: Caching & Calculator & IUserDetailsProvider
    
    public Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath)
    {
        throw new NotImplementedException();
    }

    public Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId)
    {
        throw new NotImplementedException();
    }

    public Task<UserOrganizationContext> GetUserOrganizationContextAsync()
    {
        throw new NotImplementedException();
    }

    public Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = "TenantUsers")
    {
        throw new NotImplementedException();
    }
}