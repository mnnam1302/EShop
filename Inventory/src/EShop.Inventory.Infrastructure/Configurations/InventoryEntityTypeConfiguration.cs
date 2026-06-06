using EShop.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Inventory.Infrastructure.Configurations;

internal sealed class InventoryEntityTypeConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Inventory>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Inventory> builder)
    {
        builder.ToTable(TableNames.Inventories);
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.TenantId);

        // xmin is a PostgreSQL system column — no bytea column added to schema.
        builder.UseXminAsConcurrencyToken();
    }
}