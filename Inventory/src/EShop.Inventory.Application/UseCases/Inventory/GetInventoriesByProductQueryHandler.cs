using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.Contracts.Abstractions.Pagination;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using Microsoft.EntityFrameworkCore;

namespace EShop.Inventory.Application.UseCases.Inventory;

public sealed record GetInventoriesByProductQuery(Guid ProductId, int PageIndex, int PageSize) : IQuery<PaginationResult<InventoryDto>>;

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    string Sku,
    int StockAvailable,
    int ReservedStock,
    int MinimumStock);

internal sealed class GetInventoriesByProductQueryHandler : IQueryHandler<GetInventoriesByProductQuery, PaginationResult<InventoryDto>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoriesByProductQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<PaginationResult<InventoryDto>>> HandleAsync(GetInventoriesByProductQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _inventoryRepository
            .FindByCondition(i => i.ProductId == query.ProductId)
            .OrderBy(i => i.Id);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var inventoryDtos = await queryable
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new InventoryDto(
                i.Id,
                i.ProductId,
                i.VariantId,
                i.Sku,
                i.StockAvailable,
                i.ReservedStock,
                i.MinimumStock))
            .ToListAsync(cancellationToken);

        var paginationResult = PaginationResult<InventoryDto>.Create(
            inventoryDtos,
            query.PageIndex,
            query.PageSize,
            totalCount);

        return Result.Success(paginationResult);
    }
}
