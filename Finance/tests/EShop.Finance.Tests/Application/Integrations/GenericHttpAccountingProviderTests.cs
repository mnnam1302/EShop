using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Http;
using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using EShop.Finance.Application.Services.IntegrationProvider.Generic;
using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.AccountingCompany;
using FluentAssertions;
using Moq;

namespace EShop.Finance.Tests.Application.Integrations;

public class GenericHttpAccountingProviderTests
{
    private const string Yaml = """
        triggers:
          - name: BookPayment
            actions:
              - name: Find
                request: FindInvoice
              - name: Create
                request: CreateInvoice
        requests:
          - name: FindInvoice
            urlTemplate: "{{{baseUrl}}}/invoices?ref={{{paymentId}}}"
            method: GET
          - name: CreateInvoice
            urlTemplate: "{{{baseUrl}}}/invoices"
            method: POST
        """;

    private readonly Mock<IAccountingCompanyRepository> _companies = new();
    private readonly Mock<IConnectionDetailsStore> _connectionDetails = new();
    private readonly Mock<IHttpIntegrationClient> _httpClient = new();

    public GenericHttpAccountingProviderTests()
    {
        var company = AccountingCompany.CreateDefault("tenant-1");
        company.Configure(AccountingProviderNames.GenericHttp, Yaml);
        _companies.Setup(r => r.FindByTenantIdAsync("tenant-1", It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(company);
        _connectionDetails.Setup(s => s.GetConnectionDetails("tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string?> { ["Scheme"] = "NoAuth", ["BaseUrl"] = "https://api.example.com" });
    }

    private GenericHttpAccountingProvider CreateProvider() => new(
        _companies.Object,
        _connectionDetails.Object,
        _httpClient.Object,
        new AuthenticationProviderResolver([new NoAuthAuthenticationProvider(), new BasicAuthenticationProvider()]));

    private static PaymentBookingContext Payment() => new()
    {
        TenantId = "tenant-1",
        AccountId = Guid.NewGuid(),
        OrderId = Guid.NewGuid(),
        BuyerId = "buyer-1",
        PaymentId = Guid.NewGuid(),
        Sequence = 1,
        Amount = 100m,
        Currency = "USD",
        DueDate = new DateOnly(2026, 1, 15),
    };

    private void SetupFind(IReadOnlyDictionary<string, string?>? response) =>
        _httpClient.Setup(c => c.ExecuteRequest(It.Is<RequestConfiguration>(r => r.Name == "FindInvoice"), It.IsAny<IReadOnlyDictionary<string, object?>>(), "tenant-1", It.IsAny<AuthenticationOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    private void SetupCreate(IReadOnlyDictionary<string, string?>? response) =>
        _httpClient.Setup(c => c.ExecuteRequest(It.Is<RequestConfiguration>(r => r.Name == "CreateInvoice"), It.IsAny<IReadOnlyDictionary<string, object?>>(), "tenant-1", It.IsAny<AuthenticationOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    [Fact]
    public async Task Creates_booking_when_none_exists()
    {
        SetupFind(null);
        SetupCreate(new Dictionary<string, string?> { ["bookingId"] = "INV-1" });

        var result = await CreateProvider().BookPaymentAsync(Payment(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExternalReference.Should().Be("INV-1");
        _httpClient.Verify(c => c.ExecuteRequest(It.Is<RequestConfiguration>(r => r.Name == "CreateInvoice"), It.IsAny<IReadOnlyDictionary<string, object?>>(), "tenant-1", It.IsAny<AuthenticationOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Adopts_existing_provider_booking_without_creating()
    {
        SetupFind(new Dictionary<string, string?> { ["bookingId"] = "EXISTING-1" });

        var result = await CreateProvider().BookPaymentAsync(Payment(), CancellationToken.None);

        result.Value.ExternalReference.Should().Be("EXISTING-1");
        _httpClient.Verify(c => c.ExecuteRequest(It.Is<RequestConfiguration>(r => r.Name == "CreateInvoice"), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<string>(), It.IsAny<AuthenticationOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_returning_no_booking_id_is_a_failure()
    {
        SetupFind(null);
        SetupCreate(new Dictionary<string, string?>());

        var result = await CreateProvider().BookPaymentAsync(Payment(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnection_delegates_to_the_scheme_verifier()
    {
        var ok = await CreateProvider().TestConnectionAsync(new Dictionary<string, string?> { ["Scheme"] = "Basic", ["Username"] = "user" }, CancellationToken.None);

        ok.Should().BeTrue();
    }
}
