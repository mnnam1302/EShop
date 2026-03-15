using System.ComponentModel.DataAnnotations;

namespace EShop.Catalog.ReadModels.MongoDb.Bootstrapping;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = string.Empty;
}
