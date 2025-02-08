namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public interface IUserOrganizationContextProvider
{
    Task<UserOrganizationContext> GetUserOrganizationContextAsync();
    
    Task<UserOrganizationContext> GetUserOrganizationContextForSpecificUserAsync(string userId, string typeUser = UserTypes.TenantUsers);

    Task<OrganizationContext> GetOrganizationContextForSpecificOrganizationAsync(string organizationId);

    Task<OrganizationContext> GetOrganizationContextByPathAsync(string organizationContextPath);
}

public record UserOrganizationContext
{
    public string? OrganizationContextPath { get; init; }
    public string? OrganizationId { get; init; }
    public string? OrganizationName { get; init; }
    public string? OrganizationNumber { get; init; }
    public string? OrganizationPhoneNumber { get; init; }
    public string? OrganizationAddress { get; init; }
    public string? OrganizationCity { get; init; }
    public string ? OrganizationPostcode { get; init; }
    public string? UserId { get; init; }
    public string? UserDisplayName { get; init; }
    public string? UserEmail { get; init; }
    public string? UserPhoneNumber { get; init; }
}