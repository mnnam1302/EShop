namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public interface IUserOrganizationContextCachingService
{
    Task<UserOrganizationContext?> GetValue(string userId, string userType);

    Task AddValue(string userId, string userType, UserOrganizationContext userOrganizationContext);

    Task RemoveValue(string userId, string userType);
}