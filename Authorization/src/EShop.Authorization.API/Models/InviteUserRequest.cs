namespace EShop.Authorization.API.Models;

public sealed class InviteUserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public string? PhoneNumber { get; init; }
    public required string OrganizationId { get; init; }
    public required Guid[] RoleIds { get; init; } = [];
}
