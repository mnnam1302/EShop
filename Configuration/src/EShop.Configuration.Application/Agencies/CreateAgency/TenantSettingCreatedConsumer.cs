using EShop.Configuration.Application.Shared;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using EShop.Shared.EventBus.Consumers;

namespace EShop.Configuration.Application.Agencies.CreateAgency;

public sealed class TenantSettingCreatedConsumer : Consumer<ITenantSettingCreated, ConfigurationDbContext>
{
    private readonly IMediator _mediator;

    public TenantSettingCreatedConsumer(ConfigurationDbContext dbContext, IMediator mediator) : base(dbContext)
    {
        _mediator = mediator;
    }

    protected override async Task<Result> HandleMessageAsync(ITenantSettingCreated message, CancellationToken cancellationToken)
    {
        var command = new CreateAgencyCommand
        {
            Name = message.TenantName,
            TenantId = message.TenantId
        };

        var result = await _mediator.SendAsync(command, cancellationToken);

        return result;
    }
}