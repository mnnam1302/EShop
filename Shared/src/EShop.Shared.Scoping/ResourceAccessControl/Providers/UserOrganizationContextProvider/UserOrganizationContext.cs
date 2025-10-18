namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider
{
    public sealed class UserOrganizationContext
    {
        public string UserId { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;
        public string? UserEmail { get; init; }
        public string? UserPhoneNumber { get; init; }

        public string OrganizationId { get; init; } = string.Empty;
        public string OrganizationName { get; init; } = string.Empty;
        public string? OrganizationNumber { get; init; }
        public string? OrganizationPhoneNumber { get; init; }
        public string? OrganizationEmail { get; init; }
        public string? OrganizationStreet { get; init; }
        public string? OrganizationCity { get; init; }
        public string? OrganizationCountry { get; init; }
        public string OrganizationContextPath { get; init; } = string.Empty;
    }
}
