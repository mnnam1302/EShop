using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.EventBus.Consumers;

public abstract class Consumer<TConsumer, TMessage, TDbContext>
    where TConsumer : IConsumer<TMessage>
    where TMessage : class, IMessage
    where TDbContext : DbContext, IInboxDbContext
{
    private readonly TDbContext _dbContext;

    protected Consumer(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleInboxMessage(TMessage message, CancellationToken cancellationToken = default)
    {
        //var existsingInbox = await _dbContext.InboxMessages
        //    .AnyAsync(x => x.MessageId == message.MessageId, cancellationToken);
        
        //if (!existsingInbox)
        //{

        //}
    }
}