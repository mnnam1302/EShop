using EShop.Shared.Contracts.Abstractions.Requests;
using Newtonsoft.Json;

namespace EShop.Shared.Contracts.Services.Identity.Auth;

public static class Query
{
    public sealed class Login : IQuery<Response.AuthenticatedResponse>
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }

    public sealed class Refresh : IQuery<Response.AuthenticatedResponse>
    {
        [JsonIgnore]
        public string? AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}