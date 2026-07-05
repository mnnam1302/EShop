using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Finance.Application.UseCases.ConfigureAccountingCompany;

internal sealed class ConfigureAccountingCompanyCommandHandler(
    IAccountingCompanyRepository accountingCompanies,
    IUnitOfWork unitOfWork) : ICommandHandler<ConfigureAccountingCompanyCommand>
{
    public async Task<Result> HandleAsync(ConfigureAccountingCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await accountingCompanies.FindByTenantIdAsync(command.TenantId, trackChanges: true, cancellationToken);
        if (company is null)
        {
            return Result.Failure(new Error("Finance.Company.NotFound", $"No accounting company for tenant '{command.TenantId}'."));
        }

        company.Configure(command.ProviderType, command.YamlConfiguration);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
