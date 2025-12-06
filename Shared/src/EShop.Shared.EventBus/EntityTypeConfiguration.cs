using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Shared.EventBus;

public sealed class InboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(x => x.Id);

        // This unique constraint on (MessageId, ConsumerName) in the MessageConsumers table to prevent race conditions.
        // So even if you have concurrent processing of the same message, only one will succeed in inserting the record.
        builder.HasIndex(x => new { x.MessageId, x.ConsumerId })
            .IsUnique();
    }
}