using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Order.Infrastructure.EntityConfigurations;

internal sealed class OrderEntityConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
