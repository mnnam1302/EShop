using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Shared.DomainTools.Sagas.AggregateSagas;

public abstract class AggregateSaga : Aggregate, IAggregateSaga
{
    private bool _isCompleted;
    protected virtual bool ThrowExceptionsOnFailedPublish { get; set; } = true;

    private readonly ICollection<
        Tuple<ICommand, Func<ICommandDispatcher, CancellationToken, Task<Result>>>> _unpublishedCommands = [];

    private readonly ICollection<
        Tuple<IIntegrationCommand, Func<ICommandBus, CancellationToken, Task>>> _unpublishedIntegrationCommands = [];

    public SagaState State => _isCompleted
        ? SagaState.Completed
        : IsNew ? SagaState.New : SagaState.Running;

    public bool IsCompleted()
    {
        return _isCompleted;
    }

    protected void MarkComplete()
    {
        _isCompleted = true;
    }

    protected void Publish<TCommand>(TCommand command) where TCommand : ICommand
    {
        _unpublishedCommands.Add(
            new Tuple<ICommand, Func<ICommandDispatcher, CancellationToken, Task<Result>>>(
                command,
                async (commandBus, cancellationToken) => await commandBus.DispatchAsync(command, cancellationToken)));
    }

    protected void Publish(IIntegrationCommand command)
    {
        _unpublishedIntegrationCommands.Add(
            new Tuple<IIntegrationCommand, Func<ICommandBus, CancellationToken, Task>>(
                command,
                async (commandBus, cancellationToken) => await commandBus.SendAsync(command, cancellationToken)));
    }

    public virtual async Task PublishAsync(ICommandDispatcher commandBus, CancellationToken cancellationToken)
    {
        var commandsToPublish = _unpublishedCommands.ToList();
        _unpublishedCommands.Clear();

        var exceptions = new List<CommandException>();
        foreach (var unpublishedCommand in commandsToPublish)
        {
            var command = unpublishedCommand.Item1;
            var commandInvoker = unpublishedCommand.Item2;
            if (ThrowExceptionsOnFailedPublish)
            {
                try
                {
                    var executionResult = await commandInvoker(commandBus, cancellationToken);
                    if (executionResult?.IsFailure == true)
                    {
                        exceptions.Add(
                            new CommandException(
                                command.GetType(),
                                executionResult,
                                $"Command '{command.GetType()}' published from a saga with ID '{Id}' failed with: '{executionResult}'. See ExecutionResult."));
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(
                        new CommandException(
                            command.GetType(),
                            $"Command '{command.GetType()}' published from a saga with ID '{Id}' failed with: '{e.Message}'. See InnerException.",
                            e));
                }
            }
            else
            {
                await commandInvoker(commandBus, cancellationToken);
            }
        }

        if (0 < exceptions.Count)
        {
            throw new SagaPublishException($"Some commands published from a saga with ID '{Id}' failed. See InnerExceptions.", exceptions);
        }
    }

    public virtual async Task PublishAsync(ICommandBus commandBus, CancellationToken cancellationToken)
    {
        var commandsToPublish = _unpublishedIntegrationCommands.ToList();
        _unpublishedIntegrationCommands.Clear();

        var exceptions = new List<CommandException>();
        foreach (var unpublishedCommand in commandsToPublish)
        {
            var command = unpublishedCommand.Item1;
            var commandInvoker = unpublishedCommand.Item2;
            if (ThrowExceptionsOnFailedPublish)
            {
                try
                {
                    await commandInvoker(commandBus, cancellationToken);
                }
                catch (Exception e)
                {
                    exceptions.Add(
                        new CommandException(
                            command.GetType(),
                            $"Command '{command.GetType()}' published from a saga with ID '{Id}' failed with: '{e.Message}'. See InnerException.",
                            e));
                }
            }
            else
            {
                await commandInvoker(commandBus, cancellationToken);
            }
        }

        if (0 < exceptions.Count)
        {
            throw new SagaPublishException($"Some commands published from a saga with ID '{Id}' failed. See InnerExceptions.", exceptions);
        }
    }
}
