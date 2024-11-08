using Identity.Domain.Abstractions.Entities;
using Identity.Domain.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class User : EntityBase<string>, ICreatedTracking
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

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; private set; }
    public bool IsDirector { get; set; }
    public bool IsHeadOfDepartment { get; set; }
    public string? ManagerId { get; set; }

    public string? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string? CreatedBy { get; set; }

    public bool IsActive { get; set; } = true;
}