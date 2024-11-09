using EShop.Shared.Scoping;

namespace Identity.Domain.Entities;

public class RolePermission : IExcludedFromScoping
{
    public string? RoleId { get; set; }

    public virtual Role? Role { get; set; }

    public string? PermissionId { get; set; }

    public virtual Permission? Permission { get; set; }
}