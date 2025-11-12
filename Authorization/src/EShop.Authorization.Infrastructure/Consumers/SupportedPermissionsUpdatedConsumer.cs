using EShop.Authorization.Application.UseCases.Permissions;
using EShop.Shared.Contracts.IntegrationEvents.Authorization;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Authorization.Infrastructure.Consumers;

public sealed class SupportedPermissionsUpdatedConsumer(IMediator mediator) : IConsumer<SupportedPermissionsUpdated>
{
    public async Task Consume(ConsumeContext<SupportedPermissionsUpdated> context)
    {
        var command = new UpdateSupportedPermissionsCommand
        {
            SourceSystemReference = context.Message.SourceSystemReference,
            Permissions = context.Message.Permissions,
            Action = context.Message.Action
        };

        await mediator.SendAsync(command, context.CancellationToken);
    }
}
