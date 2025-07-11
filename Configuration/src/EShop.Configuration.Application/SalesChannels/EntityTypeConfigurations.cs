using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Configuration.Application.SalesChannels;

public class SalesChannelEntityTypeConfigurations : IEntityTypeConfiguration<SalesChannel>
{
    public void Configure(EntityTypeBuilder<SalesChannel> builder)
    {
        builder.ToTable("SaleChannels");

        builder.HasKey(sc => sc.Id);

        builder.HasIndex(sc => sc.TenantId);
        builder.HasIndex(sc => sc.Scope);

        builder.HasOne(sc => sc.Agency)
            .WithMany(a => a.SalesChannels)
            .HasForeignKey(sc => sc.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sc => sc.Products)
            .WithMany(p => p.SalesChannels)
            .UsingEntity<SalesChannelProduct>();
    }
}

public class SalesChannelProductEntityTypeConfigurations : IEntityTypeConfiguration<SalesChannelProduct>
{
    public void Configure(EntityTypeBuilder<SalesChannelProduct> builder)
    {
        builder.ToTable("SalesChannelProducts");

        builder.HasKey(scp => new { scp.SalesChannelId, scp.ProductId });
    }
}