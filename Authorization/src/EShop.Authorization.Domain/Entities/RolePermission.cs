using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class RolePermission
{
    public Guid RolerId { get; set; }
    public virtual Role Role { get; set; } = null!;

    [MaxLength(ModelConstants.ShortText)]
    public string PermissionId { get; set; } = string.Empty;
    public virtual Permission Permission { get; set; } = null!;
}
