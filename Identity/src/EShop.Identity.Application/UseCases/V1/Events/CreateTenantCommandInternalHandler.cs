using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;

namespace EShop.Identity.Application.UseCases.V1.Events;

public class CreateTenantCommandInternalHandler : ICommandHandler<Command.CreateTenantCommandInternal>
{
    public Task<Result> Handle(Command.CreateTenantCommandInternal request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}