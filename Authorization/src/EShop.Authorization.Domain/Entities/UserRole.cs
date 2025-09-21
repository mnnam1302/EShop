using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class UserRole
{
    [MaxLength(ModelConstants.ShortText)]
    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}
