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
    public virtual IReadOnlyCollection<User> Users { get; private set; } = [];
    public virtual IReadOnlyCollection<UserRole> UserRoles { get; private set; } = [];

    // Permissions relationship
    public virtual IReadOnlyCollection<Permission> Permissions { get; private set; } = [];

    private readonly List<RolePermission> _rolePermissions = [];
    public virtual IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public static Role CreateOwnerRole(string tenantId)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = $"{tenantId} Owner",
            Description = "Owner role with all permissions"
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
                RoleId = Id,  // Fixed: Was RolerId
                PermissionId = permissionId
            });
        }
    }
}
