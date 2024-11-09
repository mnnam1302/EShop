using EShop.Shared.Scoping;
using Identity.Domain.Abstractions.Entities;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class Role : EntityBase<string> //, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    [Required]
    public string? Name { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public virtual List<User> Users { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    public virtual List<Permission> Permissions { get; set; } = new();
    public virtual List<RolePermission> RolePermissions { get; set; } = new();

    //[MaxLength(ModelConstants.ShortText)]
    //public string? TenantId { get; set; }

    //[MaxLength(ModelConstants.LongText)]
    //public string? Scope { get; set; }
}