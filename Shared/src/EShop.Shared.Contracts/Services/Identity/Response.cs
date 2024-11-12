namespace EShop.Shared.Contracts.Services.Identity;

public static class Response
{
    public record AuthenticatedResponse
    {
        public string UserId { get; init; }
        public string UserName { get; init; }
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public string TokenType { get; init; } = "Bearer";
        public DateTime RefreshTokenExpiryTime { get; init; }
    }

    public record RolesResponse
    {
        public string Id { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
    }
}