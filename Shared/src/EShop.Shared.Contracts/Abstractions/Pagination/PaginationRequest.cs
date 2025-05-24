namespace EShop.Shared.Contracts.Abstractions.Pagination;

public record PaginationRequest
{
    public const int UpperPageSize = 100;
    public const int DefaultPageSize = 10;
    public const int DefaultPageIndex = 1;

    private PaginationRequest(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    public static PaginationRequest Create(int pageIndex, int pageSize)
    {
        pageIndex = pageIndex <= 0 ? DefaultPageIndex : pageIndex;
        pageSize = pageSize <= 0
            ? DefaultPageSize
            : pageSize > 100
            ? UpperPageSize : pageSize;

        return new(pageIndex, pageSize);
    }

    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}