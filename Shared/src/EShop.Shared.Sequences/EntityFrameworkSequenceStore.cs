using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.Sequences;

public class EntityFrameworkSequenceStore<TDbContext> : ISequenceStore
    where TDbContext : DbContext, ISequenceDbContextStore
{
    private readonly TDbContext _dbContext;

    public EntityFrameworkSequenceStore(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegisterSequence(string sequenceId, int seedValue)
    {
        if (!await _dbContext.Sequences.AnyAsync(s => s.Id == sequenceId))
        {
            var sequence = new Sequence
            {
                Id = sequenceId,
                NextAvailableValue = seedValue
            };

            await _dbContext.Sequences.AddAsync(sequence);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<int> GetNextAvailableSequenceValue(string sequenceId, int reservedRange)
    {
        int? valueReservedForTheCaller = null;

        while (!valueReservedForTheCaller.HasValue)
        {
            try
            {
                var sequence = await _dbContext.Sequences.SingleAsync(s => s.Id == sequenceId);

                int attemptedValueReservedForTheCaller = sequence.NextAvailableValue;
                sequence.UpdateAvailableValue(reservedRange);

                await _dbContext.SaveChangesAsync();
                valueReservedForTheCaller = attemptedValueReservedForTheCaller;
            }
            catch (DbUpdateConcurrencyException)
            {
                // On optimistic concurrency we will simply try again
            }
        }

        return valueReservedForTheCaller.Value;
    }
}
