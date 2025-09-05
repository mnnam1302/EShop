using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class UserRole : IExcludedFromScoping
{
    [MaxLength(ModelConstants.MediumText)]
    public string UserId { get; set; } = string.Empty;

    public Guid RoleId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}