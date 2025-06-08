using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.Consumers;
using EShop.Tenancy.Persistence;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public class TenantFeaturesUpdatedConsumer : Consumer<ITenantFeaturesUpdated, TenancyDbContext>
{
    private readonly ISender _sender;

    public TenantFeaturesUpdatedConsumer(TenancyDbContext dbContext, ISender sender)
        : base(dbContext)
    {
        _sender = sender;
    }

    protected override async Task<Result> HandleMessageAsync(ITenantFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateTenantFeaturesCommand(message.TenantId);
        var result = await _sender.Send(command, cancellationToken);
        return result;
    }
}