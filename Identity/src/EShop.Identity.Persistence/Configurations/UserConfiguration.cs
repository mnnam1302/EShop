using EShop.Identity.Domain;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Identity.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNames.Users);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ModelConstants.MediumText)
            .IsRequired();

        builder.Property(x => x.IsDirector)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsHeadOfDepartment)
            .IsRequired()
            .HasDefaultValue(false);
    }
}