using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Consumers;
using EShop.Tenancy.Persistence;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public class SupportedFeaturesUpdatedConsumer : Consumer<ISupportedFeaturesUpdated, TenancyDbContext>
{
    private readonly ISender _sender;

    public SupportedFeaturesUpdatedConsumer(TenancyDbContext dbContext, ISender sender)
        : base(dbContext)
    {
        _sender = sender;
    }

    protected override async Task<Result> HandleMessageAsync(ISupportedFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateSupportedFeaturesInternalCommand
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