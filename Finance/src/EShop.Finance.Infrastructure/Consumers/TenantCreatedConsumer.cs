using EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Finance.Infrastructure.Consumers;

public sealed class TenantCreatedConsumer(IMediator mediator) : IConsumer<TenantCreated>
{
    public async Task Consume(ConsumeContext<TenantCreated> context)
    {
        var command = new CreateAccountingCompanyCommand
        {
            TenantId = context.Message.TenantId,
        };

        await mediator.SendAsync(command, context.CancellationToken);
    }
}
