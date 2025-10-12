using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
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

    // Users relationship
    public virtual ICollection<User> Users { get; private set; } = new List<User>();
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Permissions relationship
    public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    private readonly List<RolePermission> _rolePermissions = [];
    public virtual IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

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

    public void GrantPermissions(IEnumerable<string> enumerable)
    {
        foreach (var permissionId in enumerable)
        {
            if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
                continue;

            _rolePermissions.Add(new RolePermission
            {
                RoleId = Id,
                PermissionId = permissionId
            });
        }
    }
}
