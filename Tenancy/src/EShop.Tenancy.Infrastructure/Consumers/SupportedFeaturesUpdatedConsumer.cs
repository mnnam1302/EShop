using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Consumers;
using EShop.Tenancy.Persistence;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public class SupportedFeaturesUpdatedConsumer : Consumer<IntegrationEvent.SupportedFeaturesUpdated, TenancyDbContext>
{
    private readonly ISender _sender;

    public SupportedFeaturesUpdatedConsumer(TenancyDbContext dbContext, ISender sender)
        : base(dbContext)
    {
        _sender = sender;
    }

    protected override async Task<Result> HandleMessageAsync(IntegrationEvent.SupportedFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateSupportedFeaturesCommand
        {
            SourceSystemReference = message.SourceSystemReference,
            Features = message.Features,
            Action = message.Action,
            TenantId = message.TenantId,
            ActionUserId = message.ActionUserId
        };

        var result = await _sender.Send(command);
        return result;
    }
}