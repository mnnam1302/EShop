using EShop.Finance.Domain.Aggregates.AccountingCompany;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Finance.Infrastructure.Configurations;

internal sealed class AccountingCompanyEntityTypeConfiguration : IEntityTypeConfiguration<AccountingCompany>
{
    public void Configure(EntityTypeBuilder<AccountingCompany> builder)
    {
        builder.ToTable("AccountingCompanies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ProviderType).HasMaxLength(Domain.ModelConstants.ShortText);
        builder.Property(c => c.YamlConfiguration).HasColumnType("text");
        builder.Property(c => c.EncryptedConnectionDetails).HasColumnType("text");

        builder.HasIndex(c => c.TenantId).IsUnique();
    }
}
