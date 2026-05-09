using EShop.Catalog.ReadModels.MongoDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence.EntityConfigurations;

public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToCollection("Categories");
        builder.HasKey(c => c.Id);
    }
}
