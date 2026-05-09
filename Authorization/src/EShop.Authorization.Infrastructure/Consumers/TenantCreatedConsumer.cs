using EShop.Authorization.Application.UseCases.Organizations;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Authorization.Infrastructure.Consumers;

public sealed class TenantCreatedConsumer : IConsumer<TenantCreated>
{
    private readonly IMediator _mediator;

    public TenantCreatedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<TenantCreated> context)
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
    }
}