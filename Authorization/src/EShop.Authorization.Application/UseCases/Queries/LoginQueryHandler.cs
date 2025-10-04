using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using System.Security.Claims;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record LoginQuery(string Username, string Password) : IQuery<AuthenticationResponse>;

public sealed record AuthenticationResponse
{
    public required string UserId { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTimeOffset RefreshTokenExpiryTime { get; init; }
}

internal sealed class LoginQueryHandler : IQueryHandler<LoginQuery, AuthenticationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenManager _jwtTokenManager;
    private readonly IUserTokenCachingService _tokenCachingService;

    public LoginQueryHandler(
        IJwtTokenManager jwtTokenManager,
        IUserRepository userRepository,
        IUserTokenCachingService tokenCachingService)
    {
        _jwtTokenManager = jwtTokenManager;
        _userRepository = userRepository;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result<AuthenticationResponse>> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(query.Username) || string.IsNullOrWhiteSpace(query.Password))
        {
            return Result.Failure<AuthenticationResponse>(ErrorContants.Authentication.InvalidCredentials);
        }

        // 2. Get user information from your user store
        var user = await _userRepository.FindSingleAsync(u => u.Id == query.Username || u.Username == query.Username, cancellationToken: cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthenticationResponse>(ErrorContants.Authentication.UserNotFound);
        }

        // 3. Generate tokens using RSA asymmetric encryption
        var userClaims = BuildUserClaimsAsync(user);
        var accessToken = _jwtTokenManager.GenerateAccessToken(userClaims);
        var refreshToken = _jwtTokenManager.GenerateRefreshToken();

        // 4. Store refresh token in your data store for validation
        var result = new AuthenticationResponse
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7),
        };

        await StoreAuthenticatedResultAsync(result);

        return Result.Success(result);
    }

    private static List<Claim> BuildUserClaimsAsync(User user)
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

    private async Task StoreAuthenticatedResultAsync(AuthenticationResponse result)
    {
        var authenticationCachedValue = new AuthenticationCaching
        {
            UserId = result.UserId,
            UserName = result.UserId,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            RefreshTokenExpiryTime = result.RefreshTokenExpiryTime
        };

        await _tokenCachingService.AddTokenAsync(result.UserId, authenticationCachedValue);
    }
}
