using EShop.Authorization.Application.UseCases.Organizations.CreateRootOrganization;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Authorization.Application.Consumers;

public sealed class TenantCreatedConsumer : IConsumer<ITenantCreated>
{
    private readonly IMediator _mediator;

    public TenantCreatedConsumer(IMediator mediator)
    {
        _mediator = mediator;
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

        var result = await _mediator.SendAsync(command, context.CancellationToken);
    }
}