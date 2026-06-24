using EShop.Inventory.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Inventory.Infrastructure.Configurations;

internal sealed class ReservationItemEntityTypeConfiguration : IEntityTypeConfiguration<ReservationItem>
{
    public void Configure(EntityTypeBuilder<ReservationItem> builder)
    {
        builder.ToTable("ReservationItems");
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(ri => ri.VariantId).IsRequired();
        builder.Property(ri => ri.Quantity).IsRequired();

        builder.HasIndex(ri => new { ri.ReservationId, ri.VariantId })
            .IsUnique();

        builder.HasOne<Reservation>()
            .WithMany(r => r.Items)
            .HasForeignKey(ri => ri.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
