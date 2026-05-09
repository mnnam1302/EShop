using EShop.Catalog.Application.Shared;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Authorization;
using EShop.Shared.CQRS;

namespace EShop.Catalog.Application.Agencies.CreateAgency;

public sealed class OrganizationCreatedConsumer : Consumer<OrganizationCreated>
{
    private readonly IMediator mediator;

    public OrganizationCreatedConsumer(IMediator mediator, CatalogDbContext dbContext) : base(dbContext)
    {
        this.mediator = mediator;
    }

    protected override Task<Result> HandleMessageAsync(OrganizationCreated message, CancellationToken cancellationToken)
    {
        var command = new CreateAgencyCommand
        {
            Name = message.Name,
            TenantId = message.TenantId
        };

        return mediator.SendAsync(command, cancellationToken);
    }
}