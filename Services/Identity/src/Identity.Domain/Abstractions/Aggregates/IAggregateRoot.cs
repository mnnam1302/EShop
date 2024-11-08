using EShop.Shared.Contract.Abstractions.Requests;
using EShop.Shared.Contract.Abstractions.Messages;

namespace Identity.Domain.Abstractions.Aggregates;

public interface IAggregateRoot
{
    IEnumerable<IDomainEvent> GetDomainEvents();

    //void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents);

    void Handle(ICommand command);
}