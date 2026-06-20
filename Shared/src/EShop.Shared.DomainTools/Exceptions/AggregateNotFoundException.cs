using System.Reflection;

namespace EShop.Shared.DomainTools.Exceptions;

public sealed class AggregateNotFoundException : NotFoundException
{
    public AggregateNotFoundException(Guid aggregateId, MemberInfo aggregateType)
        : base($"{aggregateType.Name} with id {aggregateId} not found")
    {
    }
}
