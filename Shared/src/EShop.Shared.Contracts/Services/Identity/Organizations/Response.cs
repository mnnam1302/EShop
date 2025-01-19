namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class Response
{
    public record OrganizationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? OrganizationNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Postcode { get; set; }
        public string? Description { get; set; }
        public string? ParentOrganizationId { get; set; }
    }
}