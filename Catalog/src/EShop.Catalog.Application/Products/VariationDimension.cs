namespace EShop.Catalog.Application.Products;

public sealed class VariationDimension
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string[] Values { get; set; } = [];
    public VariationDisplayStyles DisplayStyle { get; set; } = VariationDisplayStyles.Text;
}
