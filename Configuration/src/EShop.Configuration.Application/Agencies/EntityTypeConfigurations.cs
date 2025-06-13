using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Configuration.Application.Agencies;

public class AgencyEntityTypeConfigurations : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("Agencies");

        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.Scope);
    }
}

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
    }
}