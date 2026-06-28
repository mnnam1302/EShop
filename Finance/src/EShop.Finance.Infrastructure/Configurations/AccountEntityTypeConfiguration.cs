using EShop.Finance.Domain.Aggregates.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Finance.Infrastructure.Configurations;

internal sealed class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(a => a.OutstandingAmount).HasColumnType("numeric(18,2)");
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(Domain.ModelConstants.ShortText);

        builder.HasIndex(a => new { a.TenantId, a.OrderId }).IsUnique();

        var instalments = builder.Metadata.FindNavigation(nameof(Account.Payments))!;
        instalments.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.Payments)
            .WithOne()
            .HasForeignKey(i => i.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
