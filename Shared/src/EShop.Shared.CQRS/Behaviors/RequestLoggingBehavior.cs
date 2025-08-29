using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Shared.CQRS.Behaviors;

internal sealed class RequestLoggingBehavior<TRequest, TResponse> //: IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
}
