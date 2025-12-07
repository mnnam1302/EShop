using EShop.Shared.DomainTools.Entities;

namespace EShop.Authorization.Domain.Entities;

public class UserRole : IExcludedFromScoping
{
    public string UserId { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
}
