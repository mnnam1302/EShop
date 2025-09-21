using EShop.Shared.DomainTools.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class Organization : AggregateRoot<string>
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? ParentOrganizationId { get; set; }
    public virtual Organization? ParentOrganization { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;
}
