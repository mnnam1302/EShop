using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EShop.Configuration.Application.Products;

public class Lookup : EntityBase<Guid>, IScoped
{
    [MaxLength(ModelConstants.MediumText)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [JsonIgnore]
    public string? DataUrl { get; set; }

    [JsonIgnore]
    public byte[]? Data { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;
}
