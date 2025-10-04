using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using System.Security.Claims;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record LoginQuery(string Username, string Password) : IQuery<AuthenticationResult>;

public sealed record AuthenticationResult
{
    public required string UserId { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTimeOffset RefreshTokenExpirationTime { get; init; }
}

public sealed class LoginQueryHandler : IQueryHandler<LoginQuery, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenManager _jwtTokenManager;

    public LoginQueryHandler(IJwtTokenManager jwtTokenManager, IUserRepository userRepository)
    {
        _jwtTokenManager = jwtTokenManager;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthenticationResult>> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(query.Username) || string.IsNullOrWhiteSpace(query.Password))
        {
            return Result.Failure<AuthenticationResult>(ErrorContants.Authentication.InvalidCredentials);
        }

        // Get user information from your user store
        var user = await _userRepository.FindSingleAsync(u => u.Id == query.Username || u.Username == query.Username, cancellationToken: cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthenticationResult>(ErrorContants.Authentication.UserNotFound);
        }

        var userClaims = BuildUserClaimsAsync(user);

        // Generate tokens using RSA asymmetric encryption
        var accessToken = _jwtTokenManager.GenerateAccessToken(userClaims);
        var refreshToken = _jwtTokenManager.GenerateRefreshToken();

        var refreshTokenExpiration = DateTimeOffset.UtcNow.AddDays(7); // 7 days default

        // Store refresh token in your data store for validation
        var result = new AuthenticationResult
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpirationTime = refreshTokenExpiration
        };

        await StoreAuthenticationResultAsync(result);

        return Result.Success(result);
    }

    private IEnumerable<Claim> BuildUserClaimsAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name),
            new("tenant_id", user.TenantId),
            new("user_type", "user"),
        };

        return claims;
    }

    private async Task StoreAuthenticationResultAsync(AuthenticationResult result)
    {

    }
}
