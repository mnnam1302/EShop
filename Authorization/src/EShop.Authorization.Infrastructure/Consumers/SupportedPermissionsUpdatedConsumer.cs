using EShop.Authorization.Application.UseCases.Commands;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Authorization.Infrastructure.Consumers;

public class SupportedPermissionsUpdatedConsumer : IConsumer<ISupportedPermissionsUpdated>
{
    private readonly IMediator _mediator;

    public SupportedPermissionsUpdatedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<ISupportedPermissionsUpdated> context)
    {
        var command = new UpdateSupportedPermissionsCommand
        {
            SourceSystemReference = context.Message.SourceSystemReference,
            Permissions = context.Message.Permissions,
            Action = context.Message.Action
        };

        await _mediator.SendAsync(command, context.CancellationToken);
    }
}
