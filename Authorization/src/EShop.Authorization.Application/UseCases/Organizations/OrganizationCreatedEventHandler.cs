namespace EShop.Authorization.Application.UseCases.Organizations;

//internal sealed class OrganizationCreatedEventHandler : IDomainEventHandler<OrganizationCreatedEvent>
//{
//    private readonly IEventBusGateway eventBus;
//    private readonly IUserDetailsProvider userDetailsProvider;

//    public OrganizationCreatedEventHandler(IEventBusGateway eventBus, IUserDetailsProvider userDetailsProvider)
//    {
//        this.eventBus = eventBus;
//        this.userDetailsProvider = userDetailsProvider;
//    }

//    public async Task Handle(OrganizationCreatedEvent domainEvent, CancellationToken cancellationToken = default)
//    {
//        var integrationEvent = new OrganizationCreated
//        {
//            OrganizationId = domainEvent.OrganizationId,
//            Name = domainEvent.Name,
//            ParentOrganizationId = domainEvent.ParentOrganizationId,
//            TenantId = domainEvent.TenantId,
//            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
//            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
//        };

//        await eventBus.PublishAsync(integrationEvent, cancellationToken);
//    }
//}
