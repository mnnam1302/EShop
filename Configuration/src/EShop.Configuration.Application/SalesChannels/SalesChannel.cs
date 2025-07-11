using EShop.Configuration.Application.Agencies;
using EShop.Configuration.Application.Products;
using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Configuration.Application.SalesChannels;

public class SalesChannel : EntityBase<Guid>, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortMediumText)]
    public string Reference { get; set; } = string.Empty;

    public Guid AgencyId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;

    public virtual Agency Agency { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = [];

    public virtual ICollection<SalesChannelProduct> SalesChannelProducts { get; set; } = [];
}