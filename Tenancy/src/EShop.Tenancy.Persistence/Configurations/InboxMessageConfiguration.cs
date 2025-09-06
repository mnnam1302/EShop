using EShop.Shared.EventBus;
using EShop.Tenancy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Tenancy.Persistence.Configurations;

internal class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable(TableNames.InboxMessages);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageType)
            .IsRequired();

        builder.Property(x => x.ConsumerId)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();
    }
}