namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider
{
    public sealed class UserOrganizationContext
    {
        public required string UserId { get; init; }
        public required string UserDisplayName { get; init; }
        public string? UserEmail { get; init; }
        public string? UserPhoneNumber { get; init; }

        public required string OrganizationId { get; init; }
        public required string OrganizationName { get; init; }
        public string? OrganizationNumber { get; init; }
        public string? OrganizationPhoneNumber { get; init; }
        public string? OrganizationEmail { get; init; }
        public string? OrganizationStreet { get; init; }
        public string? OrganizationCity { get; init; }
        public string? OrganizationCountry { get; init; }
        public required string OrganizationContextPath { get; init; }
    }
}
