using EShop.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Tenancy.Persistence.Configurations;

internal class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("TenantSettings");

        builder.HasKey(ts => ts.Id);

        builder.HasIndex(ts => ts.Scope);

        builder.OwnsOne(ts => ts.RateLimitPolicy, policy =>
        {
            policy.ToJson();
            policy.OwnsMany(p => p.Rules);
        });
    }
}
