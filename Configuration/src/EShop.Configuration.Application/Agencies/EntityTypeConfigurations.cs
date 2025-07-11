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