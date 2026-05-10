using EShop.Shared.Sequences.DependencyInjections;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Sequences;

public sealed class SequenceRange : IDisposable
{
    private readonly string _sequenceId;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private readonly IOptions<SequenceManagerOptions> _options;
    private int? _currentValue;
    private int? _maxValueReserved;

    public SequenceRange(string sequenceId, IOptions<SequenceManagerOptions> options)
    {
        _sequenceId = sequenceId;
        _options = options;
    }

    internal async Task<int> GetNextValue(ISequenceStore sequenceStore)
    {
        await _semaphoreSlim.WaitAsync();

        try
        {
            if (_currentValue is null)
            {
                var startNewRange = await GetStartNewRange(sequenceStore);
                _currentValue = startNewRange;
                UpdateReservedValue(startNewRange);
            }

            int sequenceValueForCaller = _currentValue.Value;

            if (ReachedMax())
            {
                var startNewRange = await GetStartNewRange(sequenceStore);
                _currentValue = startNewRange;
                UpdateReservedValue(startNewRange);
            }
            else
            {
                _currentValue++;
            }

            return sequenceValueForCaller;

        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task<int> GetStartNewRange(ISequenceStore sequenceStore)
    {
        var startOfNewRange = await sequenceStore
            .GetNextAvailableSequenceValue(this._sequenceId, _options.Value.ReservedSequenceValueRange);

        return startOfNewRange;
    }

    private void UpdateReservedValue(int startOfNewRange)
    {
        _maxValueReserved = startOfNewRange + _options.Value.ReservedSequenceValueRange;
    }

    private bool ReachedMax()
    {
        return _currentValue == _maxValueReserved;
    }

    public void Dispose()
    {
        _semaphoreSlim.Dispose();
    }
}
