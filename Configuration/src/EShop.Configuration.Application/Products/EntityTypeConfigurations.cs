using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Configuration.Application.Products;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.Scope);

        builder.HasOne(p => p.Agency)
            .WithMany()
            .HasForeignKey(p => p.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Versions)
            .WithOne(pv => pv.Product)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductVersionEntityTypeConfiguration : IEntityTypeConfiguration<ProductVersion>
{
    public void Configure(EntityTypeBuilder<ProductVersion> builder)
    {
        builder.ToTable("ProductVersions");

        builder.HasKey(pv => pv.Id);
        builder.HasIndex(sc => sc.TenantId);
        builder.HasIndex(sc => sc.Scope);

        builder.HasOne(pv => pv.ProductLookup)
            .WithMany()
            .HasForeignKey(pv => pv.ProductLookupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class LookupEntityTypeConfiguration : IEntityTypeConfiguration<Lookup>
{
    public void Configure(EntityTypeBuilder<Lookup> builder)
    {
        builder.ToTable("Lookups");

        builder.HasKey(pv => pv.Id);
        builder.HasIndex(sc => sc.TenantId);
        builder.HasIndex(sc => sc.Scope);
    }
}
