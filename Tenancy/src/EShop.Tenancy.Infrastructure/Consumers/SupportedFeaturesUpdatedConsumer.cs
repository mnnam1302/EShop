using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Tenancy.Persistence;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public sealed class SupportedFeaturesUpdatedConsumer : IdempotentConsumer<SupportedFeaturesUpdated>
{
    private readonly ISender _sender;

    public SupportedFeaturesUpdatedConsumer(ISender sender, TenancyDbContext dbContext) : base(dbContext)
    {
        this._sender = sender;
    }

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

        var result = await _sender.Send(command, cancellationToken);
        return result;
    }
}
