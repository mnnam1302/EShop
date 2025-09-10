using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Catalog.Application.Categories;

public class Category : AggregateRoot<Guid>, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    public string ImageUri { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;
}
