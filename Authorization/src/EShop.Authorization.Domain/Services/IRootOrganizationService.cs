using EShop.Authorization.Domain.Entities;
using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Authorization.Domain.Services;

/// <summary>
/// Domain service for managing root organization setup operations when tenant provisioned
/// </summary>
public interface IRootOrganizationService
{
    /// <summary>
    /// Sets up a complete root organization with owner role and user
    /// </summary>
    Task<Result<RootOrganizationCreation>> SetupRootOrganizationAsync(
        string tenantId,
        string tenantName,
        string ownerUsername,
        string ownerEmail,
        string ownerDisplayName,
        CancellationToken cancellationToken = default);
}

public sealed record RootOrganizationCreation(
    Organization Organization,
    Role OwnerRole,
    User OwnerUser);