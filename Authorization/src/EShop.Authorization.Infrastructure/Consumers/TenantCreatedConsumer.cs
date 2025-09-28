using EShop.Authorization.Application.UseCases.Commands;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using EShop.Shared.Scoping;
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

            var result = await _mediator.SendAsync(command, context.CancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to create root organization for tenant {TenantId}. Errors: {Errors}",
                    context.Message.TenantId,
                    result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating root organization for tenant {TenantId}", context.Message.TenantId);
            throw;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}
