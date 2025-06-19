using Microsoft.Extensions.Logging;

namespace EShop.Shared.Sequences;

public sealed class GenericSequenceRegistry : ISequenceRegistry
{
    private readonly ILogger<GenericSequenceRegistry> _logger;
    private readonly ISequenceStore _sequenceStore;

    public GenericSequenceRegistry(ILogger<GenericSequenceRegistry> logger, ISequenceStore sequenceStore)
    {
        _logger = logger;
        _sequenceStore = sequenceStore;
    }

    public async Task RegisterSequences(string applicationName, string sequenceName, int seedForSharedSequence, TenantSequence[] tenantSequences)
    {
        _logger.LogInformation("Registering required sequences...");
        await _sequenceStore.RegisterSequence(sequenceName, seedForSharedSequence);

        foreach (var tenantSequence in GetConfiguredSequences(applicationName, tenantSequences))
        {
            await _sequenceStore.RegisterSequence(tenantSequence.GetSequenceId(sequenceName), tenantSequence.SeedValue);
        }
    }

    private static IEnumerable<TenantSequence> GetConfiguredSequences(string applicationName, TenantSequence[] tenantSequences)
    {
        return tenantSequences.Where(x => x.Application == applicationName);
    }
}
