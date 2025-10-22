using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Authentication.DependencyInjections
{
    internal sealed class JwtOptions
    {
        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        [Range(1, 60)]
        public int AccessTokenExpiryMinutes { get; init; }

        [Range(1, 12)]
        public int RefreshTokenExpiryHours { get; init; }
    }
}
