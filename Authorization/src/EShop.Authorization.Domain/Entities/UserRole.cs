using EShop.Shared.DomainTools.Entities;

namespace EShop.Authorization.Domain.Entities;

public class UserRole : IExcludedFromScoping
{
    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = default!;

    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = default!;
}
