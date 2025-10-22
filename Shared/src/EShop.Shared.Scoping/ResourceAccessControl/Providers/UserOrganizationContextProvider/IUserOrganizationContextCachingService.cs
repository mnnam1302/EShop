namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider
{
    public interface IUserOrganizationContextCachingService
    {
        Task<UserOrganizationContext?> GetValue(string userId, string userType, CancellationToken cancellationToken = default);

        Task AddValue(string userId, string userType, UserOrganizationContext userOrganizationContext, CancellationToken cancellationToken = default);

        Task RemoveValue(string userId, string userType, CancellationToken cancellationToken = default);
    }
}