
using Eshop.Shared.DomainTools.Repositories;
using EShop.Identity.Domain.Abstractions.UnitOfWorks;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.Scoping;

namespace EShop.Identity.Application.UseCases.V1.Commands.Organizations
{
    public class CreateOrganizationHandler : ICommandHandler<Command.CreateOrganization>
    {
        private readonly IRepositoryBase<Organization, string> _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserDetailsProvider _userDetailsProvider;

        public CreateOrganizationHandler(
            IRepositoryBase<Organization, string> organizationRepository,
            IUnitOfWork unitOfWork,
            IUserDetailsProvider userDetailsProvider)
        {
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
            _userDetailsProvider = userDetailsProvider;
        }

        public async Task<Result> Handle(Command.CreateOrganization request, CancellationToken cancellationToken)
        {
            if (request.ParentOrganizationId != null)
            {
                await _organizationRepository.FindByIdAsync(request.ParentOrganizationId);
            }

            var organizationExists = await _organizationRepository.FindSingleAsync(x => x.Name.Equals(request.Name));
            if (organizationExists != null)
            {
                throw new ConflictException("Organization already exists");
            }

            var organization = Organization.Create(request);

            _organizationRepository.Add(organization);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}