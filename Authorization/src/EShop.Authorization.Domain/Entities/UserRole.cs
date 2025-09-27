namespace EShop.Authorization.Domain.Entities;

public class UserRole
{
    public string UserId { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
}
