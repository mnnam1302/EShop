using MassTransit;

namespace EShop.Shared.Contract.Abstractions.Messages;

[ExcludeFromTopology]
public interface ICommandBus : IMessage
{
}