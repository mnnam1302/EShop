using EShop.Authorization.Domain.Constants;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.Scoping;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Authentication;

public sealed record LogoutCommand(string UserId) : ICommand;

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

        await _userTokenCaching.RemoveAsync(command.UserId, cancellationToken);

        _logger.LogInformation("User {UserId} successfully logged out", command.UserId);
        return Result.Success();
    }
}
