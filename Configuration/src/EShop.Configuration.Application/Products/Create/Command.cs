using EShop.Configuration.Application.Agencies;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;

namespace EShop.Configuration.Application.Products.Create;

public sealed class Command : ICommand
{
    public required string Name { get; init; }
    public string? AgencyId { get; init; }
}

public sealed class CommandHandler : ICommandHandler<Command>
{
    private readonly IProductRepository productRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserDetailsProvider userDetailsProvider;
    private readonly IAgencyRepository agencyRepository;

    public CommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IUserDetailsProvider userDetailsProvider,
        IAgencyRepository agencyRepository)
    {
        this.productRepository = productRepository;
        this.unitOfWork = unitOfWork;
        this.userDetailsProvider = userDetailsProvider;
        this.agencyRepository = agencyRepository;
    }

    public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var product = Product.Create(command, userDetailsProvider);

        if (!string.IsNullOrEmpty(command.AgencyId))
        {
            var agency = await agencyRepository.FindSingleAsync(a => a.AgencyId == command.AgencyId, false, cancellationToken);
            if (agency is null)
            {
                throw new NotFoundException($"Agency with ID '{command.AgencyId}' not found.");
            }

            product.AssignToAgency(agency);
        }

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
