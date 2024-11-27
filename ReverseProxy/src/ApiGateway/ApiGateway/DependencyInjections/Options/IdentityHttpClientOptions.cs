using System.ComponentModel.DataAnnotations;

namespace ApiGateway.DependencyInjections.Options;

public class IdentityHttpClientOptions
{
    [Required, Url]
    public string BaseAddress { get; set; }

    public int ScoringApiTimeout { get; set; }
}