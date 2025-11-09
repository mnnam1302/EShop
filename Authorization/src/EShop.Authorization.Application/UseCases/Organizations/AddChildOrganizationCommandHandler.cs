using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.UseCases.Organizations;

public sealed class AddChildOrganizationCommand : ICommand
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? OrganizationNumber { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Description { get; init; }

    public string? Street { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Country { get; init; }
    public string? ZipCode { get; init; }

    public required string ParentOrganizationId { get; init; }
}

internal sealed class AddChildOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddChildOrganizationCommandHandler> logger) : ICommandHandler<AddChildOrganizationCommand>
{
    public async Task<Result> HandleAsync(AddChildOrganizationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating child organization with name: {Name}", command.Name);

        var parentOrganization = await organizationRepository.FindByIdAsync(command.ParentOrganizationId, cancellationToken: cancellationToken);
        if (parentOrganization is null)
        {
            throw new NotFoundException($"The parent organization ID '{command.ParentOrganizationId}' is not found.");
        }

        await AssertOrganizationHierarchy(parentOrganization);

        var childOrganization = parentOrganization.AddChildOrganization(
            command.Id, command.Name, command.Email,
            command.Description, command.OrganizationNumber, command.PhoneNumber, command.Street, command.City, command.State, command.Country, command.ZipCode);

        organizationRepository.Add(childOrganization);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Child organization created successfully with name: {Name}", command.Name);

        return Result.Success();
    }

    private async Task AssertOrganizationHierarchy(Organization parentOrganization)
    {
        var parentLevel = await GetOrganizationLevel(parentOrganization.Id);
        int nextLevel = parentLevel + 1;

        if (nextLevel > Organization.MaxSupportedLevel)
        {
            throw new BadRequestException($"Organization structure only supports maximum of {Organization.MaxSupportedLevel} levels.");
        }
    }

    private async Task<int> GetOrganizationLevel(string organizationId)
    {
        var level = 1;
        var organization = await organizationRepository.FindByIdAsync(organizationId);

        while (organization != null && organization.ParentOrganizationId != null)
        {
            level++;
            organization = await organizationRepository.FindByIdAsync(organization.ParentOrganizationId);
        }

        return level;
    }
}
