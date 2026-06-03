using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.Mediator;

/// <summary>
/// Synchronous command bus
/// </summary>
[ExcludeFromTopology]
public interface ICommand;

public interface ICommand<TResult>;
