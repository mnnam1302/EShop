using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Finance.API.Endpoints;

public static class AccountingCompanyEndpoints
{
    private const string BaseUrl = "api/v{version:apiVersion}/accounting-company";

    public static IEndpointRouteBuilder MapAccountingCompanyEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var group = routerBuilder
            .NewVersionedApi("AccountingCompany")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        group.MapGet("{tenantId:string}", GetAsync);
        group.MapPut(string.Empty, ConfigureAsync);
        group.MapPut("credentials", SaveCredentialsAsync);
        group.MapPost("test-connection", TestConnectionAsync);

        return routerBuilder;
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] string tenantId,
        IAccountingCompanyRepository companies,
        IUserDetailsProvider user,
        CancellationToken cancellationToken)
    {
        if (tenantId != user.AuthenticatedUser.TenantId)
        {
            Results.BadRequest("The requested tenant ID does not match the authenticated user's tenant ID.");
        }

        var company = await companies.FindByTenantIdAsync(tenantId, cancellationToken: cancellationToken);
        if (company is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            company.ProviderType,
            company.YamlConfiguration,
            HasConnectionDetails = !string.IsNullOrEmpty(company.EncryptedConnectionDetails),
        });
    }

    private static async Task<IResult> ConfigureAsync(
        [FromBody] ConfigureRequest request,
        IMediator mediator,
        IUserDetailsProvider user,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new ConfigureAccountingCompanyCommand
        {
            TenantId = user.AuthenticatedUser.TenantId,
            ProviderType = request.ProviderType,
            YamlConfiguration = request.YamlConfiguration,
        }, cancellationToken);

        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SaveCredentialsAsync(
        [FromBody] CredentialsRequest request,
        IMediator mediator,
        IUserDetailsProvider user,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new SaveConnectionDetailsCommand
        {
            TenantId = user.AuthenticatedUser.TenantId,
            ConnectionDetails = request.ConnectionDetails,
        }, cancellationToken);

        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> TestConnectionAsync(
        [FromBody] TestConnectionRequest request,
        IAccountingIntegrationProviderFactory factory,
        CancellationToken cancellationToken)
    {
        var provider = factory.GetByName(request.ProviderType);
        var valid = await provider.TestConnection(request.ConnectionDetails, cancellationToken);

        return Results.Ok(new { Valid = valid });
    }

    public sealed record ConfigureRequest(string ProviderType, string? YamlConfiguration);
    public sealed record CredentialsRequest(IReadOnlyDictionary<string, string?> ConnectionDetails);
    public sealed record TestConnectionRequest(string ProviderType, IReadOnlyDictionary<string, string?> ConnectionDetails);
}
