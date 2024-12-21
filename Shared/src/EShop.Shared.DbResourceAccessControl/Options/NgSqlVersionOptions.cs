using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.DbResourceAccessControl.Options;

public record NgSqlVersionOptions
{
    [Required]
    public int Major { get; init; } = 17;

    [Required]
    public int Minor { get; init; } = 0;
}