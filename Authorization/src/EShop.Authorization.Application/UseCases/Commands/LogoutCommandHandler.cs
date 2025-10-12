using EShop.Authorization.Domain.Constants;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Commands;

public sealed class LogoutCommand : ICommand
{
    public required string UserId { get; init; }
}

internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IUserTokenCachingService _userTokenCaching;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public LogoutCommandHandler(
        ILogger<LogoutCommandHandler> logger,
        IUserTokenCachingService userTokenCaching,
        IUserDetailsProvider userDetailsProvider)
    {
        _logger = logger;
        _userTokenCaching = userTokenCaching;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var authenticatedUserId = _userDetailsProvider.AuthenticatedUser.Id;

        if (command.UserId != authenticatedUserId)
        {
            _logger.LogWarning("User ID mismatch - command: {CommandUserId}, authenticated: {AuthenticatedUserId}",
                command.UserId, authenticatedUserId);
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        await _userTokenCaching.RemoveCacheAsync(command.UserId, cancellationToken);

        _logger.LogInformation("User {UserId} successfully logged out", command.UserId);
        return Result.Success();
    }
}
