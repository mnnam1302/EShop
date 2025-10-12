using EShop.Authorization.Domain;
using EShop.Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Authorization.Infrastructure.Configurations;

internal sealed class OrganizationEntityTypeConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => new { o.TenantId, o.Name }).IsUnique();

        builder.Property(o => o.Id)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();

        builder
            .HasOne(o => o.ParentOrganization)
            .WithMany()
            .HasForeignKey(x => x.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
