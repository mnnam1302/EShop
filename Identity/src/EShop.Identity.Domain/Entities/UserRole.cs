using EShop.Shared.Scoping;
using Microsoft.AspNetCore.Identity;

namespace EShop.Identity.Domain.Entities;

public class UserRole : IdentityUserRole<string>, IExcludedFromScoping
{
    public string UserId { get; set; }
    public virtual User User { get; set; }

    public string RoleId { get; set; }
    public virtual Role Role { get; set; }
}