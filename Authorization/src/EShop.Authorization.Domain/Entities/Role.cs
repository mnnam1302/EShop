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

    private readonly List<RolePermission> _permissions = new();
    public virtual IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

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
            if (_permissions.Any(rp => rp.PermissionId == permissionId))
                continue;

            _permissions.Add(new RolePermission
            {
                RolerId = Id,
                PermissionId = permissionId
            });
        }
    }
}
