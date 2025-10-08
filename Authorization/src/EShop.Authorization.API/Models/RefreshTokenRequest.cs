namespace EShop.Authorization.API.Models;

public sealed class RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
