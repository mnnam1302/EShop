using EShop.Shared.Contracts.Abstractions.Requests;
using Newtonsoft.Json;

namespace EShop.Shared.Contracts.Services.Identity.Auth;

public static class Query
{
    public record Login : IQuery<Response.AuthenticatedResponse>
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }

    public record Refresh : IQuery<Response.AuthenticatedResponse>
    {
        //[JsonIgnore]
        //public string? UserId { get; set; }

        [JsonIgnore]
        public string? AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}