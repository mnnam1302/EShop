using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.EventBus;

public interface IInboxDbContext
{
    DbSet<InboxMessage> InboxMessages { get; set; }
}