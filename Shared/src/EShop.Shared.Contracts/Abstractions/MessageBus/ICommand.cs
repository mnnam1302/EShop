using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus;

[ExcludeFromTopology]
public interface ICommand : IMessage
{
}