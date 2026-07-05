using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.UseCases.ProvisionAccountingCompany;

internal sealed class CreateAccountingCompanyCommandHandler(
    IAccountingCompanyRepository accountingCompanies,
    IUnitOfWork unitOfWork,
    ILogger<CreateAccountingCompanyCommandHandler> logger) : ICommandHandler<CreateAccountingCompanyCommand>
{
    public async Task<Result> HandleAsync(CreateAccountingCompanyCommand command, CancellationToken cancellationToken)
    {
        var existing = await accountingCompanies.FindByTenantIdAsync(command.TenantId, cancellationToken: cancellationToken);
        if (existing is not null)
        {
            return Result.Success();
        }

        var company = Domain.Aggregates.AccountingCompany.AccountingCompany.CreateDefault(command.TenantId);
        accountingCompanies.Add(company);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Provisioned default accounting company {CompanyId} for tenant {TenantId}.", company.Id, command.TenantId);
        return Result.Success();
    }
}
