using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.FeatureIds;

namespace EShop.Shared.Sequences;

public interface ISequenceManager
{
    Task<int> GetNextSequenceAsycn(string sequenceId);
}

internal sealed class SequenceManager : ISequenceManager
{
    private readonly Func<string, SequenceRange> _sequenceRangeFactory;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IFeatureValidator _featureValidator;
    private readonly ISequenceStore _sequenceStore;
    private readonly SequenceRangeInMemoryCache _sequenceRangeInMemoryCache;

    public SequenceManager(
        Func<string, SequenceRange> sequenceRangeFactory,
        IUserDetailsProvider userDetailsProvider,
        IFeatureValidator featureValidator,
        ISequenceStore sequenceStore,
        SequenceRangeInMemoryCache sequenceRangeInMemoryCache)
    {
        _sequenceRangeFactory = sequenceRangeFactory;
        _userDetailsProvider = userDetailsProvider;
        _featureValidator = featureValidator;
        _sequenceStore = sequenceStore;
        _sequenceRangeInMemoryCache = sequenceRangeInMemoryCache;
    }

    public async Task<int> GetNextSequenceAsycn(string sequenceId)
    {
        if (string.IsNullOrEmpty(sequenceId))
        {
            throw new ArgumentException("Sequence ID cannot be null or empty.", nameof(sequenceId));
        }

        try
        {
            var calculatedSequenceId = sequenceId;
            if (await _featureValidator.HasFeatureAsync(Authorization.EnableTenantSpecificSequences))
            {
                calculatedSequenceId = TenantSequence.GetTenantSequenceId(sequenceId, _userDetailsProvider.AuthenticatedUser.TenantId);
            }

            return await GetNextValue(calculatedSequenceId);
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("No Sequence matching", StringComparison.InvariantCultureIgnoreCase))
        {
            // fallback for safe backwards-comaptibility
            return await GetNextValue(sequenceId);
        }
    }

    private async Task<int> GetNextValue(string sequenceId)
    {
        var sequenceRange = _sequenceRangeInMemoryCache.GetOrAdd(sequenceId, _sequenceRangeFactory);
        int nextValue = await sequenceRange.GetNextValue(_sequenceStore);

        return nextValue;
    }
}