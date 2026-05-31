using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Shared.DomainTools.Sagas;

public abstract class AggregateSaga : Aggregate
{
    private bool _isCompleted;
    protected virtual bool ThrowExceptionsOnFailedPublish { get; set; } = true;

    private readonly ICollection<Tuple<ICommand, Func<ICommandDispatcher, CancellationToken, Task<Result>>>> _unpublishedCommands = [];

    public bool IsCompleted()
    {
        return _isCompleted;
    }

    protected void MarkComplete()
    {
        _isCompleted = true;
    }

    public SagaState State => _isCompleted
        ? SagaState.Completed
        : IsNew ? SagaState.New : SagaState.Running;

    protected void Publish(ICommand command)
    {
        _unpublishedCommands.Add(
            new Tuple<ICommand, Func<ICommandDispatcher, CancellationToken, Task<Result>>>(
                command,
                async (commandBus, cancellationToken) => await commandBus.DispatchAsync(command, cancellationToken)));
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
                    if (executionResult?.IsSuccess == false)
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
                await commandInvoker(commandBus, cancellationToken).ConfigureAwait(false);
            }
        }

        if (0 < exceptions.Count)
        {
            throw new SagaPublishException($"Some commands published from a saga with ID '{Id}' failed. See InnerExceptions.", exceptions);
        }
    }
}
