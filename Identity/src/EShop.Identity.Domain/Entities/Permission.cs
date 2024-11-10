using EShop.Identity.Domain.Abstractions.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Permission : EntityBase<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string? Name { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? RelatedTo { get; set; }

    public virtual List<Role> Roles { get; set; } = new();
}