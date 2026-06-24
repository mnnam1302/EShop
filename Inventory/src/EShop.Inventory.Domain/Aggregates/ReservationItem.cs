using EShop.Shared.DomainTools.Entities;

namespace EShop.Inventory.Domain.Aggregates;

public class ReservationItem : EntityBase<Guid>, IScoped
{
    public required Guid ReservationId { get; set; }
    public required Guid VariantId { get; set; }
    public required int Quantity { get; set; }

    public required string TenantId { get; set; }
    public required string Scope { get; set; }
}
