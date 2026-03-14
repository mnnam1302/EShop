using EShop.Authorization.Application.UseCases.Organizations;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Infrastructure.Consumers;

public sealed class TenantCreatedConsumer : IConsumer<ITenantCreated>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantCreatedConsumer> _logger;

    public TenantCreatedConsumer(IMediator mediator, ILogger<TenantCreatedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ITenantCreated> context)
    {
        var command = new CreateRootOrganizationCommand
        {
            TenantId = context.Message.TenantId,
            TenantName = context.Message.TenantName,
            OwnerUsername = context.Message.OwnerUsername,
            OwnerDisplayName = context.Message.OwnerDisplayName,
            OwnerEmail = context.Message.OwnerEmail
        };

        await _mediator.SendAsync(command, context.CancellationToken);

        // TODO: Improvement: Use a Result pattern for error handling,
        // or catch only intentional exceptions (e.g., DomainException),
        // then log them as warnings instead of errors.
    }
}
