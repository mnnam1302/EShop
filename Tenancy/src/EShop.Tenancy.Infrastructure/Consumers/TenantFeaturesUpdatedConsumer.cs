using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.CQRS;
using EShop.Tenancy.Application.UseCases.Tenants.ClearTenantFeatures;
using EShop.Tenancy.Persistence;

namespace EShop.Tenancy.Infrastructure.Consumers;

public sealed class TenantFeaturesUpdatedConsumer : IdempotentConsumer<TenantFeaturesUpdated>
{
    private readonly IMediator _mediator;

    public TenantFeaturesUpdatedConsumer(IMediator sender, TenancyDbContext dbContext) : base(dbContext)
    {
        _mediator = sender;
    }

    protected override async Task<Result> HandleMessageAsync(TenantFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new ClearTenantFeaturesCommand
        {
            TenantId = message.TenantId
        };

        var result = await _mediator.SendAsync(command, cancellationToken);

        return result;
    }
}
