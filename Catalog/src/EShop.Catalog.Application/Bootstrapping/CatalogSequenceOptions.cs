using EShop.Shared.Sequences;

namespace EShop.Catalog.Application.Bootstrapping;

public class CatalogSequenceOptions
{
    public const string SectionName = "CatalogReference";

    public int CategoryReferenceSeed { get; set; } = 500000;

    public TenantSequence[] TenantSequences { get; set; } = [];
}
