using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Application.Services;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRsaKeyManager _rsaKeyManager;

    public LoginQueryHandler(
        IJwtTokenManager jwtTokenManager,
        IUserRepository userRepository,
        IUserTokenCachingService tokenCachingService,
        IPasswordHasher passwordHasher,
        IRsaKeyManager rsaKeyManager)
    {
        _jwtTokenManager = jwtTokenManager;
        _userRepository = userRepository;
        _tokenCachingService = tokenCachingService;
        _passwordHasher = passwordHasher;
        _rsaKeyManager = rsaKeyManager;
    }

    public async Task<Result<AuthenticationResponse>> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(query.Username) || string.IsNullOrWhiteSpace(query.Password))
        {
            return Result.Failure<AuthenticationResponse>(ErrorContants.Authentication.InvalidCredentials);
        }

        // 2. Authenticate user credentials
        var userResult = await AuthenticateUserAsync(query, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(userResult.Error);
        }

        var user = userResult.Value;

        // 3. Ensure RSA key pair exists for tenant (auto-generated if needed)
        await EnsureRsaKeyPairExistsAsync(user.TenantId);

        // 4. Generate RSA-signed JWT tokens
        var tokenResult = await GenerateAuthenticationTokensAsync(user, cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(tokenResult.Error);
        }

        // 5. Cache authentication tokens for session management
        await StoreAuthenticatedResultAsync(tokenResult.Value);

        return Result.Success(tokenResult.Value);
    }

    private async Task<Result<User>> AuthenticateUserAsync(LoginQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindSingleAsync(
            u => u.Id == query.Username || u.Username == query.Username,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.Failure<User>(ErrorContants.Authentication.UserNotFound);
        }

        var isPasswordValid = _passwordHasher.VerifyHashedPassword(user.HashedPassword, query.Password);
        if (!isPasswordValid)
        {
            return Result.Failure<User>(ErrorContants.Authentication.InvalidPassword);
        }

        return Result.Success(user);
    }

    private async Task EnsureRsaKeyPairExistsAsync(string tenantId)
    {
        var existingKeyPair = await _rsaKeyManager.GetActiveKeyPairAsync(tenantId);
        if (existingKeyPair is null || existingKeyPair.ExpiresAt <= DateTimeOffset.UtcNow.AddDays(7))
        {
            await _rsaKeyManager.GenerateKeyPairAsync(tenantId);
        }
    }

    private async Task<Result<AuthenticationResponse>> GenerateAuthenticationTokensAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            var userClaims = BuildUserClaims(user);
            var accessToken = await _jwtTokenManager.GenerateAccessTokenAsync(userClaims, user.TenantId, cancellationToken);
            var refreshToken = _jwtTokenManager.GenerateRefreshToken();

            var result = new AuthenticationResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7)
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthenticationResponse>(new Error("TokenGeneration.Failed", ex.Message));
        }
    }

    private static List<Claim> BuildUserClaims(User user)
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
        var authenticationCachedValue = new TokenAuthenticationCaching
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
