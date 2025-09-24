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

    public static Permission Create(string id, string name, string relatedTo, string? description = null)
    {
        return new Permission
        {
            Id = id,
            Name = name,
            RelatedTo = relatedTo,
            Description = description
        };
    }
}
