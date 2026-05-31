namespace EShop.Shared.DomainTools.Exceptions;

public sealed class SagaPublishException : Exception
{
    public IReadOnlyCollection<Exception> InnerExceptions { get; }

    public SagaPublishException(string message, IReadOnlyCollection<Exception> innerExceptions)
        : base(message)
    {
        InnerExceptions = innerExceptions;
    }
}
