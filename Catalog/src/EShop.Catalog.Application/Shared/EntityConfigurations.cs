using EShop.Shared.DomainTools.EventSourcing.Converters;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.Sequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Catalog.Application.Shared;

public sealed class SequenceEntityTypeConfiguration : IEntityTypeConfiguration<Sequence>
{
    public void Configure(EntityTypeBuilder<Sequence> builder)
    {
        builder.ToTable("Sequences");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.NextAvailableValue)
            .IsRequired();

        builder.Property(s => s.ConcurrencyToken)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();
    }
}