using EShop.Authorization.Application.UseCases.Commands;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Authorization.Application.Consumers;

public class SupportedPermissionsUpdatedConsumer(IMediator mediator) : IConsumer<ISupportedPermissionsUpdated>
{
    public async Task Consume(ConsumeContext<ISupportedPermissionsUpdated> context)
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
