namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public interface IOrganizationContextCachingService
{
    Task<OrganizationContext?> GetValue(string organizationId, CancellationToken cancellationToken = default);

    Task AddValue(string organizationId, OrganizationContext organizationContext, CancellationToken cancellationToken = default);

    Task RemoveValue(string organizationId, CancellationToken cancellationToken = default);
}