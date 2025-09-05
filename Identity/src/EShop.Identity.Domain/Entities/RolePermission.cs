using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class RolePermission : IExcludedFromScoping
{
    public Guid RoleId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string PermissionId { get; set; } = string.Empty;

    public virtual Role Role { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;
}