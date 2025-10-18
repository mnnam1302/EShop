namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider
{
    public sealed class OrganizationContext
    {
        public required string OrganizationId { get; init; }
        public required string OrganizationName { get; init; }
        public string? OrganizationNumber { get; init; }
        public string? OrganizationPhoneNumber { get; init; }
        public string? OrganizationEmail { get; init; }
        public string? OrganizationAddress { get; init; }
        public string? OrganizationCity { get; init; }
        public string? OrganizationPostcode { get; init; }
        public required string OrganizationContextPath { get; init; }
    }
}
