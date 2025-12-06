using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Abstractions;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public sealed class TenantFeaturesUpdatedConsumer(
    IMessageRepository messageRepository,
    ISender sender) : IdempotentConsumer<ITenantFeaturesUpdated>(messageRepository)
{
    protected override async Task<Result> HandleMessageAsync(ITenantFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateTenantFeaturesCommand(message.TenantId);

        var result = await sender.Send(command, cancellationToken);

        return result;
    }
}