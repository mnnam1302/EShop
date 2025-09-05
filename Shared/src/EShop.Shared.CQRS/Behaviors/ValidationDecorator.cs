using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using FluentValidation;

namespace EShop.Shared.CQRS.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _innerHandler;
        private readonly IEnumerable<IValidator<TCommand>> _validators;

        public CommandHandler(
            ICommandHandler<TCommand> innerHandler,
            IEnumerable<IValidator<TCommand>> validators)
        {
            _innerHandler = innerHandler;
            _validators = validators;
        }

        public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            Error[] errors = await ValidateAsync(command, _validators, cancellationToken);
            if (errors.Length != 0)
            {
                return ValidationResult.WithErrors(errors);
            }

            return await _innerHandler.HandleAsync(command, cancellationToken);
        }
    }


    internal sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : Result
    {
        private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
        private readonly IEnumerable<IValidator<TCommand>> _validators;

        public CommandHandler(
            ICommandHandler<TCommand, TResponse> innerHandler,
            IEnumerable<IValidator<TCommand>> validators)
        {
            _innerHandler = innerHandler;
            _validators = validators;
        }

        public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            Error[] errors = await ValidateAsync(command, _validators, cancellationToken);
            if (errors.Length != 0)
            {
                return ValidationResult<TResponse>.WithErrors(errors);
            }

            return await _innerHandler.HandleAsync(command, cancellationToken);
        }
    }

    private static async Task<Error[]> ValidateAsync<TCommand>(TCommand command, IEnumerable<IValidator<TCommand>> validators, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TCommand>(command);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context)));

        return validationResults
            .SelectMany(r => r.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(failure => new Error(
                failure.PropertyName,
                failure.ErrorMessage))
            .Distinct()
            .ToArray();
    }
}
