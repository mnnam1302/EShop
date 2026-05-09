using EShop.Shared.Authentication;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Inventory.Domain.Entities;

public class Inventory : AggregateRoot<Guid>, IScoped, IAuditable
{
    public required Guid ProductId { get; set; }

    public required Guid SkuId { get; set; } // variant id

    [MaxLength(ModelConstants.MediumText)]
    public required string Sku { get; set; }

    public int StockAvailable { get; set; }

    public int ReservedStock { get; set; }

    public int MinimumStock { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public required string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public required string Scope { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? LastModifiedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public static Inventory Create(
        Guid productId,
        Guid skuId,
        string sku,
        int stockAvailable,
        int minimumStock,
        UserData currentUser)
    {
        return new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SkuId = skuId,
            Sku = sku,
            StockAvailable = stockAvailable,
            MinimumStock = minimumStock,
            TenantId = currentUser.TenantId,
            Scope = currentUser.TenantId,
            CreatedByUserId = currentUser.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

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
