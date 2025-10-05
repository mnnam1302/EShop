using EShop.Identity.Application.Abstractions;
using EShop.Shared.Scoping.DependencyInjections.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EShop.Identity.Infrastructure.Authentication;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions = new JwtOptions();

    public TokenService(IConfiguration configuration)
    {
        configuration.GetSection(nameof(JwtOptions)).Bind(_jwtOptions);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var screteKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var signatureCredentials = new SigningCredentials(screteKey, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(_jwtOptions.AccessTokenExpiryMinutes),
            signingCredentials: signatureCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var Key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Key),
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCulture))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}