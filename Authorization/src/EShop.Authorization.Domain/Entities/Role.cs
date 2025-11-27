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

    public virtual ICollection<User> Users { get; private set; } = new List<User>();
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    private readonly List<RolePermission> rolePermissions = [];
    public virtual IReadOnlyCollection<RolePermission> RolePermissions => rolePermissions.AsReadOnly();

    public static Role CreateOwnerRole(string tenantId)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = $"Role Owner",
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
            if (rolePermissions.Any(rp => rp.PermissionId == permissionId))
            {
                continue;
            }

            rolePermissions.Add(new RolePermission
            {
                RoleId = Id,
                PermissionId = permissionId
            });
        }
    }
}
