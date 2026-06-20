using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.EventBus;

public sealed class CommandBus(IBus bus) : ICommandBus
{
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IIntegrationCommand
    {
        var endpoint = await bus.GetSendEndpoint(GetAddress(command));
        await endpoint.Send(command, cancellationToken);
    }

    private static Uri GetAddress(IIntegrationCommand command)
        => new($"exchange:{KebabCaseEndpointNameFormatter.Instance.SanitizeName(command.GetType().Name)}");
}
