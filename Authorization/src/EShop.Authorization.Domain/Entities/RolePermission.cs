using EShop.Shared.DomainTools.Entities;

namespace EShop.Authorization.Domain.Entities;

public class RolePermission : IExcludedFromScoping
{
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = default!;

    public string PermissionId { get; set; } = string.Empty;
    public virtual Permission Permission { get; set; } = default!;
}
