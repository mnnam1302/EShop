using EShop.Shared.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.EventBus;

internal sealed class PostgreSqlMessageRepository<TDbContext>(TDbContext dbContext) : IMessageRepository
    where TDbContext : DbContext, IInboxDbContext
{
    public async Task AddAsync(InboxMessage inboxMessage, CancellationToken cancellationToken)
    {
        dbContext.InboxMessages.Add(inboxMessage);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid messageId, string consumerId, CancellationToken cancellationToken)
    {
        return await dbContext.InboxMessages
            .AnyAsync(m => m.Id == messageId && m.ConsumerId == consumerId, cancellationToken);
    }
}