using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.DomainEvents;
using EShop.Shared.CQRS.DomainEvent;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Users;

internal sealed class UserCreatedDomainEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserCreatedDomainEventHandler> _logger;

    public UserCreatedDomainEventHandler(IEmailService emailService, ILogger<UserCreatedDomainEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var welcomeEmail = EmailMetaDataFactory.CreateWelcomeEmail(
            domainEvent.Email,
            domainEvent.Username,
            domainEvent.RawPassword);

        await _emailService.SendAsync(welcomeEmail, cancellationToken);

        _logger.LogInformation("Welcome email sent to user {UserId}", domainEvent.UserId);
    }
}
