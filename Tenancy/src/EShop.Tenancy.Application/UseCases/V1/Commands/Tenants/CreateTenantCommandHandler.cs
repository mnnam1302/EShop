using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.DomainExceptions;
using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Aggregates;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Application.UseCases.V1.Commands.Tenants;

public class CreateTenantCommandHandler : ICommandHandler<Command.CreateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenancyUnitOfWork _tenancyUnitOfWork;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository, ITenancyUnitOfWork tenancyUnitOfWork)
    {
        _tenantRepository = tenantRepository;
        _tenancyUnitOfWork = tenancyUnitOfWork;
    }

    public async Task<Result> Handle(Command.CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existsingTenant = await _tenantRepository.FindSingleAsync(x => x.Name == request.Name);
        if (existsingTenant is not null)
        {
            throw new BadRequestException("Tenant with the same name already exists.");
        }

        var tenant = Tenant.Create(request);

        _tenantRepository.Add(tenant);
        await _tenancyUnitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}