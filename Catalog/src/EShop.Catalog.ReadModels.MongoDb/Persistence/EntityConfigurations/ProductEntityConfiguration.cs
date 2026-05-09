using EShop.Catalog.ReadModels.MongoDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence.EntityConfigurations;

public sealed class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToCollection("Products");
        builder.HasKey(p => p.Id);
    }
}
