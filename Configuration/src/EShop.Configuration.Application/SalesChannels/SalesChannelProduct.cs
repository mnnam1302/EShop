using EShop.Configuration.Application.Products;
using EShop.Shared.Scoping;

namespace EShop.Configuration.Application.SalesChannels;

public class SalesChannelProduct : IExcludedFromScoping
{
    public Guid SalesChannelId { get; set; }

    public Guid ProductId { get; set; }

    public virtual SalesChannel SalesChannel { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
