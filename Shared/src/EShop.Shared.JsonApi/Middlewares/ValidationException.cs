using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Shared.JsonApi.Middlewares;

public class ValidationException : DomainException
{
    public ValidationException(IReadOnlyCollection<Error> errors)
        : base("Validation Failure", "One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<Error> Errors { get; }
}