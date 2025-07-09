using EShop.Shared.Sequences.DependencyInjections;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Sequences;

public sealed class SequenceRange : IDisposable
{
    private readonly string sequenceId;
    private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
    private readonly IOptions<SequenceManagerOptions> options;
    private int? currentValue;
    private int? maxValueReserved;

    public SequenceRange(string sequenceId, IOptions<SequenceManagerOptions> options)
    {
        this.sequenceId = sequenceId;
        this.options = options;
    }

    internal async Task<int> GetNextValue(ISequenceStore sequenceStore)
    {
        await this.semaphoreSlim.WaitAsync().ConfigureAwait(false);

        try
        {
            if (this.currentValue is null)
            {
                var startNewRange = await GetStartNewRange(sequenceStore);
                currentValue = startNewRange;
                UpdateReservedValue(startNewRange);
            }

            int sequenceValueForCaller = currentValue.Value;

            if (ReachedMax())
            {
                var startNewRange = await GetStartNewRange(sequenceStore);
                currentValue = startNewRange;
                UpdateReservedValue(startNewRange);
            }
            else
            {
                currentValue++;
            }

            return sequenceValueForCaller;

        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }

    private async Task<int> GetStartNewRange(ISequenceStore sequenceStore)
    {
        var startOfNewRange = await sequenceStore
            .GetNextAvailableSequenceValue(this.sequenceId, options.Value.ReservedSequenceValueRange);

        return startOfNewRange;
    }

    private void UpdateReservedValue(int startOfNewRange)
    {
        maxValueReserved = startOfNewRange + options.Value.ReservedSequenceValueRange;
    }

    private bool ReachedMax()
    {
        return currentValue == maxValueReserved;
    }

    public void Dispose()
    {
        semaphoreSlim.Dispose();
    }
}