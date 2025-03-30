using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Role : EntityBase<string>, IScoped
{
    public const string OwnerRoleName = "Owner";

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; private set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; private set; }

    public virtual List<User> Users { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    public virtual List<Permission> Permissions { get; set; } = new();
    public virtual List<RolePermission> RolePermissions { get; set; } = new();

    [MaxLength(ModelConstants.ShortText)]
    public string? TenantId { get; private set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; private set; }

    public Role() { }

    private Role(Guid id, string name, string tenantId)
    {
        Id = $"role-{id}";
        Name = name;
        TenantId = tenantId;
        Scope = tenantId;
    }

    public static Role Create(string name, string? description, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Role name cannot empty");
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "TenantId cannot empty");
        }

        var role = new Role(Guid.NewGuid(), name, tenantId)
        {
            Description = description
        };

        return role;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void GrantPermission(string permissionId)
    {
        var rolePermission = new RolePermission
        {
            RoleId = Id,
            PermissionId = permissionId
        };

        RolePermissions.Add(rolePermission);
    }
}