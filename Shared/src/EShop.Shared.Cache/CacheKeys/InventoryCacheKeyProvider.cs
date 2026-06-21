namespace EShop.Shared.Cache.CacheKeys;

public static class InventoryCacheKeyProvider
{
    private const string OwnerService = "inventory";

    public static string GetAvailableStockKey(Guid variantId) => $"{OwnerService}:stock_available:{variantId}";

    public static string GetReservedStockKey(Guid variantId) => $"{OwnerService}:stock_reserved:{variantId}";
}
