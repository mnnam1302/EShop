using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Authentication.Filters;

/// <summary>
/// MassTransit consume filter that automatically sets auth context from IIntegrationEvent messages.
/// Ensures SetSystemUserContext is called before consumer logic and ClearSystemUserContext is called after.
/// </summary>
public sealed class SystemUserContextConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class, IIntegrationEvent
{
    private readonly IUserDetailsProvider _userDetailsProvider;

    public SystemUserContextConsumeFilter(IUserDetailsProvider userDetailsProvider)
    {
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        try
        {
            // Extract auth context from integration event message
            var message = context.Message;
            _userDetailsProvider.SetSystemUserContext(
                message.TenantId,
                message.ActionUserId,
                message.ActionUserType);

            // Delegate to next pipe (the actual consumer)
            await next.Send(context);
        }
        finally
        {
            // Always clear context, even if consumer throws
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("systemUserContext");
    }
}
