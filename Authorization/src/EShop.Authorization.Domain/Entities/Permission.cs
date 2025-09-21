using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class Permission : EntityBase<string>
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string RelatedTo { get; set; } = string.Empty;
}
