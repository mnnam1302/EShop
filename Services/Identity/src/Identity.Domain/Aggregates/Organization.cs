using EShop.Shared.Contract.Abstractions.Messages;
using EShop.Shared.Contract.Abstractions.Requests;
using Identity.Domain.Abstractions.Aggregates;
using Identity.Domain.Abstractions.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Identity.Domain.Aggregates;

public class Organization : AggregateRoot
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid ParentOrganizationId { get; set; }
    public string OrganizationNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string SenderEmail { get; set; }

    public override void Handle(ICommand command)
    {
        throw new NotImplementedException();
    }

    protected override void Apply(IDomainEvent domainEvent)
    {
        throw new NotImplementedException();
    }
}