using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.EventSourcing.Converters;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Shared.DomainTools.EventSourcing.Configurations
{
    public sealed class SnapshotEntityTypeConfiguration : IEntityTypeConfiguration<Snapshot>
    {
        public void Configure(EntityTypeBuilder<Snapshot> builder)
        {
            builder.ToTable("Snapshots");

            builder.HasKey(s => s.Id);
            builder.HasIndex(s => new { s.AggregateId, s.Version })
                .IsUnique();

            builder.Property(s => s.Id)
                .IsRequired();

            builder.Property(s => s.AggregateId)
                .IsRequired();

            builder.Property(s => s.AggregateType)
                .HasMaxLength(ModelConstants.MediumText)
                .IsRequired();

            builder.Property(s => s.Version)
                .IsRequired();

            builder.Property(s => s.Aggregate)
                .HasConversion<AggregateConverter>()
                .IsRequired();

            builder.Property(s => s.CreatedOnUtc)
                .IsRequired();
        }
    }
}