using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.Contracts.Abstractions.Pagination;

public class PaginationResult<T>
{
    public List<T> Items { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex * PageSize < TotalCount;

    public PaginationResult(List<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PaginationResult<T> Create(List<T> items,
        int pageIndex, 
        int pageSize, 
        int totalCount)
    {
        return new (items, pageIndex, pageSize, totalCount);
    }

    public static async Task<PaginationResult<T>> CreateAsync(IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new (items, pageIndex, pageSize, totalCount);
    }
}