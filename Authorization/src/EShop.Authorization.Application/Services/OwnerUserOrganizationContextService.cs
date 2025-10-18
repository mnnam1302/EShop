using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Authorization.Application.Services;

internal sealed class OwnerUserOrganizationContextService : IUserOrganizationContextProvider
{
    //private readonly IUserOrganizationContextCalculator _userOrganizationContext;

    //public OwnerUserOrganizationContextService(IUserOrganizationContextCalculator userOrganizationContext)
    //{
    //    _userOrganizationContext = userOrganizationContext;
    //}

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
