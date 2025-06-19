namespace EShop.Shared.Sequences;

public interface ISequenceRegistry
{
    Task RegisterSequences(string applicationName, string sequenceName, int seedForSharedSequence, TenantSequence[] tenantSequences);
}