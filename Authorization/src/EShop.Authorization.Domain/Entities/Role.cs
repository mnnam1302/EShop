using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class Role : EntityBase<Guid>, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    public virtual ICollection<User> Users { get; private set; } = [];
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public virtual ICollection<Permission> Permissions { get; private set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];

    public static Role CreateOwnerRole(string tenantId)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = $"Role owner {tenantId}",
            Description = "Owner role with all permissions",
            TenantId = tenantId,
            Scope = tenantId
        };

        return role;
    }

    public static Role Create(string name, string? description, string tenantId)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            TenantId = tenantId,
            Scope = tenantId
        };
        return role;
    }

    public void GrantPermissions(IEnumerable<string> enumerable)
    {
        foreach (var permissionId in enumerable)
        {
            if (RolePermissions.Any(rp => rp.PermissionId == permissionId))
            {
                continue;
            }

            RolePermissions.Add(new RolePermission
            {
                RoleId = Id,
                PermissionId = permissionId
            });
        }
    }
}
