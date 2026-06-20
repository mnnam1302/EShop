using EShop.Inventory.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Inventory.Infrastructure.Configurations;

internal sealed class ReservationEntityTypeConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.OrderId);
        //builder.HasIndex(r => new { r.Status, r.ExpiresAt });
    }
}
