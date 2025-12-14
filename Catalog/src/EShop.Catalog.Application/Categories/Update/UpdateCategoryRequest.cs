namespace EShop.Catalog.Application.Categories.Update;

public sealed class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}
