using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.Mediator;

[ExcludeFromTopology]
public interface ICommand;

public interface ICommand<TResult>;
