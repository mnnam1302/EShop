using EShop.Identity.Domain.Abstractions.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class ApplicationUser : EntityBase<string> //, IScoped // multi tenant later
{
    [MaxLength(ModelConstants.ShortText)]
    public string? UserId { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? Username { get; set; }

    [MaxLength(ModelConstants.StandardText)]
    public string? DisplayName { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? CustomerId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationId { get; set; }

    public virtual Organization? Organization { get; set; }

    public bool? CanReInvite { get; set; }

    //[MaxLength(ModelConstants.ShortText)]
    //public string? TenantId { get; set; }

    //[MaxLength(ModelConstants.LongText)]
    //public string? Scope { get; set; }
}