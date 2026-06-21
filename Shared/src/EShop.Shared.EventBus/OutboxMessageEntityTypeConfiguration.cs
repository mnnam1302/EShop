using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Shared.EventBus;

public sealed class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AggregateId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AggregateName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventName).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.ProcessedOnUtc);
    }
}
