using EShop.Shared.CQRS.Command;

namespace EShop.Shared.DomainTools.Sagas;

public interface ISaga
{
    SagaState State { get; }

    Task PublishAsync(ICommandDispatcher commandBus, CancellationToken cancellationToken);
}
