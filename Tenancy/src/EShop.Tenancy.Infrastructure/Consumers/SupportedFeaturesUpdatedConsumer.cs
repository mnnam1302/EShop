using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public sealed class SupportedFeaturesUpdatedConsumer(
    IMessageRepository messageRepository,
    ISender sender) : IdempotentConsumer<SupportedFeaturesUpdated>(messageRepository)
{
    protected override async Task<Result> HandleMessageAsync(SupportedFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateSupportedFeaturesInternalCommand
        {
            SourceSystemReference = message.SourceSystemReference,
            Features = message.Features,
            Action = message.Action,
            TenantId = message.TenantId,
            ActionUserId = message.ActionUserId
        };

        var result = await sender.Send(command, cancellationToken);
        return result;
    }
}