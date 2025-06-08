using EShop.Shared.Scoping;

namespace EShop.Identity.Domain.Entities;

public class UserRole : IExcludedFromScoping
{
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}