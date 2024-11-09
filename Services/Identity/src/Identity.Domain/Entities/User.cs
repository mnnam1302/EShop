using Identity.Domain.Abstractions.Entities;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class User : EntityBase<string>, ICreatedTracking //, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    [Required]
    public string? Username { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? DisplayName { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(ModelConstants.StandardText)]
    public string PasswordHash { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; private set; }
    public bool IsDirector { get; set; }
    public bool IsHeadOfDepartment { get; set; }
    public string? ManagerId { get; set; }

    public string? OrganizationId { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string? CreatedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual List<Role> Roles { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    //[MaxLength(ModelConstants.ShortText)]
    //public string? TenantId { get; set; }

    //[MaxLength(ModelConstants.LongText)]
    //public string? Scope { get; set; }
}