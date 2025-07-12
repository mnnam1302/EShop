using EShop.Identity.Persistence;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Permissions;
using EShop.Shared.EventBus.Consumers;
using MediatR;

namespace EShop.Identity.Infrastructure.Consumers;

public class SupportedPermissionsUpdatedConsumer : Consumer<ISupportedPermissionsUpdated, UsersDbContext>
{
    private readonly ISender _sender;

    public SupportedPermissionsUpdatedConsumer(UsersDbContext dbContext, ISender sender)
        : base(dbContext)
    {
        _sender = sender;
    }

    protected override async Task<Result> HandleMessageAsync(ISupportedPermissionsUpdated message, CancellationToken cancellationToken)
    {
        var command = new Command.UpdateSupportedPermissionsCommandInternal
        {
            SourceSystemReference = message.SourceSystemReference,
            Permissions = message.Permissions,
            Action = message.Action
        };

        var result = await _sender.Send(command);
        return result;
    }
}