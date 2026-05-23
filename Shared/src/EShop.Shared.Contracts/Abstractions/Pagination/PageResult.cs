using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.Contracts.Abstractions.Pagination;

public class PaginationResult<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex * PageSize < TotalCount;

    private PaginationResult(IReadOnlyCollection<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PaginationResult<T> Create(IReadOnlyCollection<T> items, int pageIndex, int pageSize, int totalCount)
    {
        return new (items, pageIndex, pageSize, totalCount);
    }
}
