using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Http;
using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Finance.Application.Services.IntegrationProvider.TemplateData;
using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.AccountingCompany;
using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Finance.Application.Services.IntegrationProvider.Generic;

public sealed class GenericHttpAccountingProvider(
    IAccountingCompanyRepository accountingCompanies,
    IConnectionDetailsStore connectionDetailsStore,
    IHttpIntegrationClient httpClient,
    IAuthenticationProviderResolver authenticationResolver) : IAccountingIntegrationProvider
{
    private const string BookingIdKey = "bookingId";

    public string Name => AccountingProviderNames.GenericHttp;

    public async Task<Result<PaymentBookingResult>> BookPaymentAsync(PaymentBookingContext context, CancellationToken cancellationToken)
    {
        var httpContext = await BuildContext(context.TenantId, cancellationToken);
        var templateModel = PaymentBookingTemplateModel.Parse(context, httpContext.Configuration.DateFormat ?? "yyyy-MM-dd");
        var templateData = templateModel.GetTemplateDataModel();

        try
        {
            if (httpContext.GetFindPaymentRequest() is { } findRequest)
            {
                var found = await httpClient.ExecuteRequest(findRequest, templateData, context.TenantId, httpContext.AuthOptions, cancellationToken);
                var existingReference = GetBookingId(found);
                if (!string.IsNullOrEmpty(existingReference))
                {
                    return Result.Success(new PaymentBookingResult(existingReference));
                }
            }

            var createRequest = httpContext.GetCreatePaymentRequest()
                ?? throw new InvalidOperationException("Provider configuration has no 'BookPayment/Create' request.");

            var created = await httpClient.ExecuteRequest(createRequest, templateData, context.TenantId, httpContext.AuthOptions, cancellationToken);
            var bookingReference = GetBookingId(created);

            return string.IsNullOrEmpty(bookingReference)
                ? Result.Failure<PaymentBookingResult>(new Error("Finance.Provider.NoBookingId", $"Provider returned no booking id for payment {context.PaymentId}."))
                : Result.Success(new PaymentBookingResult(bookingReference));
        }
        catch (ServerCommunicationException ex)
        {
            return Result.Failure<PaymentBookingResult>(new Error("Finance.Provider.CommunicationFailed", ex.Message));
        }
    }

    public async Task<bool> TestConnectionAsync(IReadOnlyDictionary<string, string?> connectionDetails, CancellationToken cancellationToken)
    {
        try
        {
            var options = AuthenticationOptions.Create(connectionDetails);
            var authProvider = authenticationResolver.Resolve(options.Scheme);
            return await authProvider.VerifyAuthentication(options, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ServerCommunicationException)
        {
            return false;
        }
    }

    private async Task<HttpAccountingContext> BuildContext(string tenantId, CancellationToken cancellationToken)
    {
        var accountingCompany = await accountingCompanies.FindByTenantIdAsync(tenantId, cancellationToken: cancellationToken);
        if (accountingCompany is null)
        {
            throw new InvalidOperationException($"No accounting company for tenant '{tenantId}'.");
        }

        var connectionDetails = await connectionDetailsStore.GetConnectionDetails(tenantId, cancellationToken);
        if (connectionDetails is null)
        {
            throw new InvalidOperationException($"No connection details configured for tenant '{tenantId}'.");
        }

        var configuration = ProviderConfigurationParser.Parse(accountingCompany.YamlConfiguration);
        var authOptions = AuthenticationOptions.Create(connectionDetails);

        return new HttpAccountingContext(configuration, authOptions);
    }

    private static string? GetBookingId(IReadOnlyDictionary<string, string?>? response)
        => response is not null && response.TryGetValue(BookingIdKey, out var value) ? value : null;
}
