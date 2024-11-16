namespace EShop.Shared.Contracts.Services.Identity.Roles;

public static class Response
{
    public record RolesResponse
    {
        public string Id { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
    }
}