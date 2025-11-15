using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Domain.StateMachines;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Users;

public sealed class ConfirmInvitationCommand : ICommand
{
    public required string Username { get; init; }
    public required string TemporaryPassword { get; init; }
    public required string NewPassword { get; init; }
}

internal sealed class ConfirmInvitationCommandHandler(
    ILogger<ConfirmInvitationCommandHandler> logger,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<ConfirmInvitationCommand>
{
    public async Task<Result> HandleAsync(ConfirmInvitationCommand command, CancellationToken cancellationToken)
    {
        // 1. check existing user with username
        var user = await userRepository.FindSingleAsync(predicate: u => u.Username == command.Username, trackChanges: true, cancellationToken: cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User with username {Username} not found", command.Username);
            return Result.Failure(new Error("User.NotFound", $"User with the specified username {command.Username} was not found."));
        }

        // 2. check user is pending verification
        if (!user.StateMachine.CanFire(UserAction.ConfirmInvitation))
        {
            logger.LogWarning("User with username {Username} is not in a state to confirm invitation", command.Username);
            return Result.Failure(new Error("User.InvalidState", $"User with username {command.Username} is not in a state to confirm invitation."));
        }

        // 3. verify temporary password
        var tempPasswordVerificationResult = passwordHasher.VerifyHashedPassword(user.PasswordHash, command.TemporaryPassword);
        if (!tempPasswordVerificationResult)
        {
            logger.LogWarning("Temporary password verification failed for user {Username}", command.Username);
            return Result.Failure(new Error("User.InvalidTemporaryPassword", "The provided temporary password is incorrect."));
        }

        // 4. behave confirm invitation
        user.ConfirmInvitation(passwordHasher.Hash(command.NewPassword));

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
