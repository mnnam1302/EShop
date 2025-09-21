using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class Role : EntityBase<Guid>
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
