using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Authorization;
using EShop.Shared.CQRS;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Agencies.CreateAgency;

public sealed class OrganizationCreatedConsumer(
    IMessageRepository messageRepository,
    IMediator mediator) 
    : IdempotentConsumer<OrganizationCreated>(messageRepository)
{
    protected override Task<Result> HandleMessageAsync(OrganizationCreated message, CancellationToken cancellationToken)
    {
        var command = new CreateAgencyCommand
        {
            Name = message.Name,
            TenantId = message.TenantId
        };

        return mediator.SendAsync(command, cancellationToken);
    }
}