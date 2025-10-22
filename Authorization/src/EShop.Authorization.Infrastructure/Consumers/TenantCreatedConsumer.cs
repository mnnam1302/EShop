using EShop.Authorization.Application.UseCases.Organizations;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Infrastructure.Consumers;

public sealed class TenantCreatedConsumer : IConsumer<ITenantCreated>
{
    private readonly IMediator _mediator;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<TenantCreatedConsumer> _logger;

    public TenantCreatedConsumer(IMediator mediator, IUserDetailsProvider userDetailsProvider, ILogger<TenantCreatedConsumer> logger)
    {
        _mediator = mediator;
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ITenantCreated> context)
    {
        _userDetailsProvider.SetSystemUserContext(context.Message.TenantId);

        try
        {
            var command = new CreateRootOrganizationCommand
            {
                TenantId = context.Message.TenantId,
                TenantName = context.Message.TenantName,
                OwnerUsername = context.Message.OwnerUsername,
                OwnerDisplayName = context.Message.OwnerDisplayName,
                OwnerEmail = context.Message.OwnerEmail
            };

            await _mediator.SendAsync(command, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating root organization for tenant {TenantId}", context.Message.TenantId);
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}
