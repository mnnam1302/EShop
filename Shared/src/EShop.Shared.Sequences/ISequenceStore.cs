namespace EShop.Shared.Sequences;

public interface ISequenceStore
{
    Task RegisterSequence(string sequenceId, int seedValue);

    Task<int> GetNextAvailableSequenceValue(string sequenceId, int reservedRange);
}