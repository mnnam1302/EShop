using EShop.Identity.Application.Abstractions;
using EShop.Identity.Infrastructure.DependencyInjections.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EShop.Identity.Infrastructure.Authentication;

public class TokenService : ITokenService
{
    private readonly JwtOptions jwtOptions = new JwtOptions();

    public TokenService(IConfiguration configuration)
    {
        configuration.GetSection(nameof(JwtOptions)).Bind(jwtOptions);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var screteKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
        var signatureCredentials = new SigningCredentials(screteKey, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            //expires: DateTime.Now.AddHours(jwtOptions.AccessExpireHour),
            expires: DateTime.UtcNow.AddSeconds(30),
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
        var Key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

        // Cấu hình như th Server Valdate ở JwtExtensions.cs
        // Đầu tiên, phải kiểm tra token Expire này có đúng token mà mình đã cấp phát hay không - đúng cái secret key hay không?
        // Lỡ ngta fake rồi sao?
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateLifetime = false, // here we are saying that we don't care about the token's expiration date
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