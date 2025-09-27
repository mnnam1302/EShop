namespace EShop.Authorization.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public string PermissionId { get; set; } = string.Empty;
}
