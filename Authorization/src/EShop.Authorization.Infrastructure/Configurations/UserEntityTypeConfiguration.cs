using EShop.Authorization.Domain;
using EShop.Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Authorization.Infrastructure.Configurations;

internal sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Username).IsUnique();

        builder.Property(x => x.Id)
               .HasMaxLength(ModelConstants.MediumText)
               .IsRequired();

        builder.HasOne(u => u.Organization)
               .WithMany()
               .HasForeignKey(u => u.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserRoles)
               .WithOne(ur => ur.User)
               .HasForeignKey(ur => ur.UserId);
    }
}