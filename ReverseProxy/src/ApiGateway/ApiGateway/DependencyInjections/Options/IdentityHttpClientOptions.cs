using System.ComponentModel.DataAnnotations;

namespace ApiGateway.DependencyInjections.Options;

public record IdentityHttpClientOptions
{
    [Required, Url]
    public required string BaseAddress { get; init; }
}