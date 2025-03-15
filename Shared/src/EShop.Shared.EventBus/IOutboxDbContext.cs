using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.EventBus;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; set; }
}