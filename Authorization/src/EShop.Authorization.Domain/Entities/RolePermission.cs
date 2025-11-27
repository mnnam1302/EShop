using EShop.Shared.DomainTools.Entities;

namespace EShop.Authorization.Domain.Entities;

public class RolePermission : IExcludedFromScoping
{
    public Guid RoleId { get; set; }
    public string PermissionId { get; set; } = string.Empty;
}
