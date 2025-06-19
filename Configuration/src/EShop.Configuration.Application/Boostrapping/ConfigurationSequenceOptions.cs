using EShop.Shared.Sequences;

namespace EShop.Configuration.Application.Boostrapping;

public class ConfigurationSequenceOptions
{
    public const string SectionName = "ConfigurationReference";

    public int SalesChannelReferenceSeed { get; set; } = 500000;

    public TenantSequence[] TenantSequences { get; set; } = [];
}
