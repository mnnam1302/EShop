namespace EShop.Shared.Sequences;

internal class GenericUniqueReferenceGenerator : IUniqueReferenceGenerator
{
    private readonly ISequenceManager _sequenceManager;

    public GenericUniqueReferenceGenerator(ISequenceManager sequenceManager)
    {
        _sequenceManager = sequenceManager;
    }

    public async Task<string> CreateReference(string sequenceName)
    {
        int nextSequenceNumber = await _sequenceManager.GetNextSequenceAsycn(sequenceName);
        return nextSequenceNumber.ToString();
    }
}