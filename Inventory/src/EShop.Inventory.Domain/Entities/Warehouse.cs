using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Inventory.Domain.Entities;

public sealed class Warehouse : AggregateRoot<Guid>, IScoped
{
    public required string TenantId { get; set; }

    public required string Scope { get; set; }
}