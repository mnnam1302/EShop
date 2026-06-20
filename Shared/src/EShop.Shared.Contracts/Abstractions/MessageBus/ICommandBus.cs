namespace EShop.Shared.Contracts.Abstractions.MessageBus;

public interface ICommandBus
{
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : IIntegrationCommand;
}
