using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public interface IOrganizationContextCachingService
{
    Task<OrganizationContext?> GetValue(string organizationId);

    Task AddValue(string organizationId, OrganizationContext organizationContext);

    Task RemoveValue(string organizationId);
}