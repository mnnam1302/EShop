using System.Collections.Concurrent;

namespace EShop.Shared.Sequences;

internal class SequenceRangeInMemoryCache
{
    private readonly ConcurrentDictionary<string, SequenceRange> sequenceRanges = new();

    public SequenceRange GetOrAdd(string sequenceId, Func<string, SequenceRange> sequenceRangeFactory)
    {
        return sequenceRanges.GetOrAdd(sequenceId, id => sequenceRangeFactory(id));
    }
}
