using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.EventSourcing.Converters;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Shared.DomainTools.EventSourcing.Configurations;

public sealed class EventStoreEntityTypeConfiguration : IEntityTypeConfiguration<EventStore>
{
    public void Configure(EntityTypeBuilder<EventStore> builder)
    {
        builder.ToTable("EventStores");

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique();

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.AggregateId)
            .IsRequired();

        builder.Property(e => e.AggregateType)
            .HasMaxLength(ModelConstants.MediumText)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(ModelConstants.MediumText)
            .IsRequired();

        builder.Property(e => e.Event)
            .HasConversion<EventConverter>()
            .IsRequired();

        builder.Property(e => e.CreatedOnUtc)
            .IsRequired();
    }
}
