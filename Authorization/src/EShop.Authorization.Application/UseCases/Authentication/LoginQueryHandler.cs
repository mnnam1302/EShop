using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Authorization.Application.UseCases.Authentication;

public sealed record LoginQuery(string Username, string Password) : IQuery<AuthenticationResponse>;

public sealed class AuthenticationResponse
{
    public required string UserId { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTimeOffset RefreshTokenExpiryTime { get; init; }
}

internal sealed class LoginQueryHandler(
    IJwtTokenManager jwtTokenManager,
    IUserRepository userRepository,
    IUserTokenCachingService tokenCachingService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IQueryHandler<LoginQuery, AuthenticationResponse>
{
    public async Task<Result<AuthenticationResponse>> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Authenticate user credentials (returns unified errors)
        var signIn = await PasswordSignInAsync(query, cancellationToken);
        
        if (signIn.IsFailure) 
            return Result.Failure<AuthenticationResponse>(signIn.Error);

        var user = signIn.Value;

        // 2. Generate tokens (use config)
        var tokens = await GenerateTokensAsync(user, cancellationToken);

        // 3. Store auth session (cache)
        await StoreSessionAsync(user.Username, tokens, cancellationToken);

        return Result.Success(tokens);
    }

    private async Task<Result<User>> PasswordSignInAsync(LoginQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(predicate: u => u.Username == query.Username, cancellationToken: cancellationToken);

        if (user == null)
            return Result.Failure<User>(ErrorContants.Authentication.InvalidCredentials);

        if (user.StateMachine.IsInState(Domain.StateMachines.UserState.PendingVerification))
            return Result.Failure<User>(ErrorContants.User.PendingVerification);

        if (user.IsLockedOut())
            return Result.Failure<User>(ErrorContants.User.LockedOut);

        if (!passwordHasher.CheckPassword(user.PasswordHash, query.Password))
        {
            var failedCount = user.IncrementAccessFailedCount();

            if (failedCount >= User.MaxFailedAccessAttemptsBeforeLockout)
            {
                user.SetLockout(DateTimeOffset.UtcNow.Add(User.DefaultAccountLockoutTimeSpan));
                user.ResetAccessFailedCount();
            }

            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<User>(ErrorContants.Authentication.InvalidCredentials);
        }

        user.ResetAccessFailedCount();
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user);
    }

    private async Task<AuthenticationResponse> GenerateTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = await jwtTokenManager.GenerateAccessToken(user.Id, user.TenantId, cancellationToken: cancellationToken);
        var refreshToken = jwtTokenManager.GenerateRefreshToken();
        //var expiry = DateTimeOffset.UtcNow.Add(authSettings.RefreshTokenLifetime);

        return new AuthenticationResponse
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7)
        };
    }

    private async Task StoreSessionAsync(string username, AuthenticationResponse result, CancellationToken cancellationToken)
    {
        var cachedValue = new TokenAuthentication
        {
            UserId = result.UserId,
            UserName = username,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            RefreshTokenExpiryTime = result.RefreshTokenExpiryTime
        };

        await tokenCachingService.AddAsync(result.UserId, cachedValue, cancellationToken);
    }
}