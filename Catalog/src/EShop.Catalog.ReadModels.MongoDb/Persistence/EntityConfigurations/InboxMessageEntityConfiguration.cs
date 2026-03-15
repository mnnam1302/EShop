using EShop.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence.EntityConfigurations;

public sealed class InboxMessageEntityConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToCollection("InboxMessages");
        builder.HasKey(m => m.Id);
    }
}
