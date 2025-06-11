using EShop.Shared.Contracts.Shared;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Response
{
    public record UserOrganizationContext
    {
        public string? OrganizationId { get; init; }
        public string? OrganizationName { get; init; }
        public string? OrganizationNumber { get; init; }
        public string? OrganizationPhoneNumber { get; init; }
        public string? OrganizationEmail { get; init; }
        public string? OrganizationAddress { get; init; }
        public string? OrganizationCity { get; init; }
        public string? OrganizationPostcode { get; init; }
        public string? OrganizationContextPath { get; init; }
        public string? UserId { get; init; }
        public string? UserDisplayName { get; init; }
        public string? UserEmail { get; init; }
        public string? UserPhoneNumber { get; init; }
    }

    public record UserPermissionsResponse
    {
        public string[]? Permissions { get; init; }
    }

    public sealed record UserDetailsResponse
    {
        public string Id { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string? DisplayName { get; init; }
        public string Email { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? OrganizationId { get; init; }

        public Dictionary<string, ActionDefinition> Actions { get; init; } = [];
    }
}