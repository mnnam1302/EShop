namespace EShop.Authorization.API.Models;

public sealed class LogoutRequest
{
    public required string UserId { get; init; }
}
