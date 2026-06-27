using EShop.Finance.Domain.Aggregates.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Finance.Infrastructure.Configurations;

internal sealed class PaymentEntityTypeConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Amount).HasColumnType("numeric(18,2)");
        builder.Property(i => i.PaidAmount).HasColumnType("numeric(18,2)");
        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(Domain.ModelConstants.ShortText);

        builder.HasIndex(i => i.AccountId);
        builder.HasIndex(i => new { i.AccountId, i.Sequence }).IsUnique();
    }
}
