using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Authentication;

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

    public LoginQueryHandler(
        IJwtTokenManager jwtTokenManager,
        IUserRepository userRepository,
        IUserTokenCachingService tokenCachingService,
        IPasswordHasher passwordHasher)
    {
        _jwtTokenManager = jwtTokenManager;
        _userRepository = userRepository;
        _tokenCachingService = tokenCachingService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthenticationResponse>> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        var inputValidation = ValidateLoginInput(query);
        if (inputValidation.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(inputValidation.Error);
        }

        // 2. Authenticate user credentials
        var userResult = await AuthenticateUserAsync(query, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(userResult.Error);
        }

        var user = userResult.Value;

        // 3. Generate RSA-signed JWT tokens
        var tokenResult = await GenerateAuthenticationTokensAsync(user, cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(tokenResult.Error);
        }

        // 4. Cache authentication tokens for session management
        await StoreAuthenticatedResultAsync(tokenResult.Value, cancellationToken);

        return Result.Success(tokenResult.Value);
    }

    private static Result ValidateLoginInput(LoginQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Username) || string.IsNullOrWhiteSpace(query.Password))
        {
            return Result.Failure(ErrorContants.Authentication.InvalidCredentials);
        }

        return Result.Success();
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

        if (user.StateMachine.IsInState(Domain.StateMachines.UserState.PendingVerification))
        {
            return Result.Failure<User>(ErrorContants.Authentication.UserPendingVerification);
        }

        var isPasswordValid = _passwordHasher.VerifyHashedPassword(user.PasswordHash, query.Password);
        if (!isPasswordValid)
        {
            return Result.Failure<User>(ErrorContants.Authentication.InvalidPassword);
        }

        return Result.Success(user);
    }

    private async Task<Result<AuthenticationResponse>> GenerateAuthenticationTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = await _jwtTokenManager.GenerateAccessToken(user.Id, user.TenantId, cancellationToken: cancellationToken);
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

    private async Task StoreAuthenticatedResultAsync(AuthenticationResponse result, CancellationToken cancellationToken)
    {
        var authenticationCachedValue = new TokenAuthentication
        {
            UserId = result.UserId,
            UserName = result.UserId,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            RefreshTokenExpiryTime = result.RefreshTokenExpiryTime
        };

        await _tokenCachingService.AddAsync(result.UserId, authenticationCachedValue, cancellationToken);
    }
}
