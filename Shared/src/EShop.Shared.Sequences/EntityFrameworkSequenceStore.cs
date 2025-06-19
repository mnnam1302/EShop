using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.Sequences;

public class EntityFrameworkSequenceStore<TDbContext> : ISequenceStore
    where TDbContext : DbContext, ISequenceDbContextStore
{
    private readonly ILogger _logger;
    private readonly TDbContext _dbContext;

    public EntityFrameworkSequenceStore(ILogger logger, TDbContext dbContext)
    {
        _logger = logger;
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
            _logger.LogInformation("Registered sequence '{SequenceId}' with a seed value of '{SeedValue}'", sequenceId, seedValue);
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
