using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Permission : EntityBase<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string RelatedTo { get; set; } = string.Empty;

    public virtual List<Role> Roles { get; set; } = [];
}