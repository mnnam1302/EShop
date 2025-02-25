using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Tenancy.Persistence.Configurations;

internal class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
{
    public void Configure(EntityTypeBuilder<TenantFeature> builder)
    {
        builder.ToTable(TableNames.TenantFeatures);
        
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.FeatureId });
        
        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.TenantFeatures)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Feature)
            .WithMany()
            .HasForeignKey(x => x.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}