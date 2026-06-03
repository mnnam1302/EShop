using EShop.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Inventory.Infrastructure.Configurations;

internal sealed class ReservationEntityTypeConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("StockReservations");
        builder.HasKey(r => r.Id);

        // Index for fast lookup by order (e.g. compensation path).
        builder.HasIndex(r => r.OrderId);

        //builder.HasIndex(r => new { r.Status, r.ExpiresAt });

        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.VariantId).IsRequired();
        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
    }
}
