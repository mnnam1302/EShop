using EShop.Configuration.Application.Agencies;
using EShop.Configuration.Application.SalesChannels;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;

namespace EShop.Configuration.Application.Products;

public class Product : EntityBase<Guid>, IScoped
{
    public string Name { get; set; } = string.Empty;

    public string? AgencyId { get; set; }

    public virtual Agency? Agency { get; set; }

    //public string? VersionIdForSales { get; set; }

    //public virtual ProductVersion? ActiveVersion { get; set; }

    public bool IsActive { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset? ArchivedDate { get; set; }

    public DateTimeOffset? UnarchivedDate { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public string? LastModifiedByUserId { get; set; } = string.Empty;

    public string TenantId { get; private set; } = string.Empty;

    public string Scope { get; private set; } = string.Empty;

    public virtual ICollection<SalesChannel> SalesChannels { get; set; } = [];

    public virtual ICollection<SalesChannelProduct> SalesChannelProducts { get; set; } = [];

    public virtual ICollection<ProductVersion> Versions { get; set; } = [];
}