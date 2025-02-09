namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;

public record OrganizationContext
{
    public string? OrganizationContextPath { get; init; }
    public string? OrganizationId { get; init; }
    public string? OrganizationName { get; init; }
    public string? OrganizationNumber { get; init; }
    public string? OrganizationPhoneNumber { get; init; }
    public string? OrganizationAddress { get; init; }
    public string? OrganizationCity { get; init; }
    public string? OrganizationPostcode { get; init; }
}