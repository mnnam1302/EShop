using EShop.Shared.DomainTools.Entities;

namespace EShop.Inventory.Domain.Entities;

public sealed class Inventory : EntityBase<Guid>
{
    public required string Sku { get; set; }

    public required int Quantity { get; set; }
}