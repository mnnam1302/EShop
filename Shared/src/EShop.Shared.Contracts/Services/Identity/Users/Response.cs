namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class Response
{
    public record UserPermissionsResponse
    {
        public string[]? Permissions { get; init; }
    }
}