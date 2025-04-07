using EShop.Identity.Persistence;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.EventBus.Consumers;
using MediatR;

namespace EShop.Identity.Infrastructure.Consumers;

public static class TenantConsumers
{
    public class TenantCreatedConsumer : Consumer<Shared.Contracts.Services.Tenancy.Tenants.TenantCreated, UsersDbContext>
    {
        private readonly ISender _sender;

        public TenantCreatedConsumer(UsersDbContext dbContext, ISender sender) : base(dbContext)
        {
            _sender = sender;
        }

        protected override async Task<Result> HandleMessageAsync(
            Shared.Contracts.Services.Tenancy.Tenants.TenantCreated message,
            CancellationToken cancellationToken)
        {
            var command = new Command.CreateTenantCommandInternal(
                message.TenantId,
                message.TenantName,
                message.OwnerUsername,
                message.OwnerDisplayName,
                message.OwnerEmail);
            var result = await _sender.Send(command, cancellationToken);
            return result;
        }
    }
}