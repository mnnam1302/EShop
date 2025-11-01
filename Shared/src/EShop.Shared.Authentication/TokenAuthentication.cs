namespace EShop.Shared.Authentication;

public sealed class TokenAuthentication
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public string Type { get; init; } = "Bearer";
    public DateTimeOffset RefreshTokenExpiryTime { get; init; }
}
