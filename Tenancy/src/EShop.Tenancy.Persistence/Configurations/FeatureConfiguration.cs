using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Tenancy.Persistence.Configurations;

internal class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable(TableNames.Features);

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Description);
        builder.Property(x => x.Module).IsRequired();
        builder.Property(x => x.State).IsRequired();
    }
}