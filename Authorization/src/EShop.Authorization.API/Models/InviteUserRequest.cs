namespace EShop.Authorization.API.Models;

public sealed class InviteUserRequest
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string OrganizationId { get; set; }
    public required Guid[] RoleIds { get; set; } = [];
}
