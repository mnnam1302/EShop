using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.CQRS;
using EShop.Tenancy.Application.UseCases.Features.UpdateFeatures;
using EShop.Tenancy.Persistence;

namespace EShop.Tenancy.Infrastructure.Consumers;

public sealed class SupportedFeaturesUpdatedConsumer : IdempotentConsumer<SupportedFeaturesUpdated>
{
    private readonly IMediator _mediator;

    public SupportedFeaturesUpdatedConsumer(TenancyDbContext dbContext, IMediator mediator) : base(dbContext)
    {
        _mediator = mediator;
    }

    protected override async Task<Result> HandleMessageAsync(SupportedFeaturesUpdated message, CancellationToken cancellationToken)
    {
        var command = new UpdateSupportedFeaturesCommand
        {
            SourceSystemReference = message.SourceSystemReference,
            Features = message.Features,
            Action = message.Action,
            TenantId = message.TenantId,
            ActionUserId = message.ActionUserId
        };

        var result = await _mediator.SendAsync(command, cancellationToken);
        return result;
    }
}
