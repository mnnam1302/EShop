using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Inventory.Domain.Entities;

public class Inventory : AggregateRoot<Guid>, IScoped
{
    public required Guid SkuId { get; set; } // variant id from catalog bounded context
    public required int StockAvailable { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public required string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public required string Scope { get; set; }

    public void RecieveStock()
    {
    }

    public void ReserveStock()
    {
    }

    public void ReleaseStock()
    {
    }

    public void DeductStock()
    {
    }

    public void AdjustStock()
    {
    }
}