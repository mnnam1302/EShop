using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Tenancy.Persistence;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public sealed class TenantFeaturesUpdatedConsumer : IdempotentConsumer<TenantFeaturesUpdated>
{
    private readonly ISender sender;

    public TenantFeaturesUpdatedConsumer(ISender sender, TenancyDbContext dbContext) : base(dbContext)
    {
        this.sender = sender;
    }

    protected override async Task<Result> HandleMessageAsync(TenantFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateTenantFeaturesCommand(message.TenantId);

        var result = await sender.Send(command, cancellationToken);

        return result;
    }
}