using EShop.Shared.Scoping;

namespace EShop.Identity.Domain.Entities;

public class UserRole : IExcludedFromScoping
{
    public string? UserId { get; set; }
    public virtual User? User { get; set; }

    public string RoleId { get; set; }
    public virtual Role? Role { get; set; }
}