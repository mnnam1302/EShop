using EShop.Finance.Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Finance.Infrastructure.Configurations;

internal sealed class IntegrationProviderSessionEntityTypeConfiguration : IEntityTypeConfiguration<IntegrationProviderSession>
{
    public void Configure(EntityTypeBuilder<IntegrationProviderSession> builder)
    {
        builder.ToTable("IntegrationProviderSessions");
        builder.HasKey(s => s.TenantId);

        builder.Property(s => s.TenantId).HasMaxLength(Domain.ModelConstants.ShortText);
        builder.Property(s => s.SessionToken).HasColumnType("text");
    }
}
