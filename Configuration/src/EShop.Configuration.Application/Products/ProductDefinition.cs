using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShop.Configuration.Application.Products;

[Owned]
public class ProductDefinition
{
    [JsonProperty("productJson")]
    public string ProductJson { get; set; } = string.Empty;

    [NotMapped]
    [JsonProperty("compilationErrors")]
    public string[]? CompilationErrors { get; set; }

    [JsonIgnore]
    public string? CompilationErrorsJson
    {
        get => (CompilationErrors == null) ? null : JsonConvert.SerializeObject(CompilationErrors);
        set => CompilationErrors = (value == null) ? null : JsonConvert.DeserializeObject<string[]>(value);
    }

    [JsonProperty("isValid")]
    public bool IsValid => CompilationErrors == null || CompilationErrors.Length == 0;
}
