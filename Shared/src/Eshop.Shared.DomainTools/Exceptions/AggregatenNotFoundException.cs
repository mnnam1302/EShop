using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.DomainTools.Exceptions;

public sealed class AggregatenNotFoundException : NotFoundException
{
    public AggregatenNotFoundException(Guid aggregateId, MemberInfo aggregateType) 
        : base($"{aggregateType.Name} with id {aggregateId} not found")
    {
    }
}
