using System.Reflection;

namespace EShop.Shared.DomainTools.Exceptions;

public sealed class AggregatenNotFoundException : NotFoundException
{
    public AggregatenNotFoundException(Guid aggregateId, MemberInfo aggregateType)
        : base($"{aggregateType.Name} with id {aggregateId} not found")
    {
    }
}