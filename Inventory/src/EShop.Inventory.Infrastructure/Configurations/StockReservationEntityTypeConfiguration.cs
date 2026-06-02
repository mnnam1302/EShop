using EShop.Inventory.Domain.Entities;
using EShop.Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Inventory.Infrastructure.Configurations;

internal sealed class StockReservationEntityTypeConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.VariantId).IsRequired();
        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.IdempotencyKey).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Unique index enforces per-order idempotency for active reservations.
        // Partial index (WHERE Status != 'Released') is expressed via a filter.
        builder.HasIndex(r => r.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"Status\" NOT IN ('Released', 'Expired')");

        // Composite index for the expiry-job query.
        builder.HasIndex(r => new { r.Status, r.ExpiresAt });

        // Index for fast lookup by order (e.g. compensation path).
        builder.HasIndex(r => r.OrderId);
    }
}
