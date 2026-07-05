using EShop.Finance.Application.Services.IntegrationProvider.Http;
using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using FluentAssertions;
using static EShop.Finance.Application.Services.IntegrationProvider.Configuration.FinanceConfigurationConstants;

namespace EShop.Finance.Tests.Application.Integrations.YamlConfigurations;

public class YamlConfigurationTests
{
    private const string ValidYaml = """
        dateFormat: "yyyy-MM-dd"
        triggers:
          - name: BookPayment
            actions:
              - name: Find
                request: FindInvoice
              - name: Create
                request: CreateInvoice
        requests:
          - name: FindInvoice
            urlTemplate: "{{{baseUrl}}}/invoices?reference={{{paymentId}}}"
            method: GET
            responseTemplate: |
              { "bookingId": "{{{id}}}" }
          - name: CreateInvoice
            urlTemplate: "{{{baseUrl}}}/invoices"
            method: POST
            requestTemplate: |
              { "amount": {{{amount}}}, "currency": "{{{currency}}}" }
            responseTemplate: |
              { "bookingId": "{{{id}}}" }
        """;

    [Fact]
    public void Parses_valid_configuration()
    {
        var config = ProviderConfigurationParser.Parse(ValidYaml);

        config.Triggers.Should().HaveCount(1);
        config.Requests.Should().HaveCount(2);
        config.GetRequestConfiguration(Triggers.BookPayment, Actions.Create)!.Name.Should().Be("CreateInvoice");
        config.GetRequestConfiguration(Triggers.BookPayment, Actions.Find)!.Method.Should().Be("GET");
    }

    [Fact]
    public void Blank_configuration_parses_to_empty()
    {
        ProviderConfigurationParser.Parse("  ").Triggers.Should().BeEmpty();
    }

    [Fact]
    public void Malformed_configuration_throws()
    {
        var act = () => ProviderConfigurationParser.Parse("triggers: [ unclosed");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Missing_action_resolves_to_null()
    {
        var config = ProviderConfigurationParser.Parse(ValidYaml);

        config.GetRequestConfiguration(Triggers.BookPayment, "DoesNotExist").Should().BeNull();
    }

    [Fact]
    public void Handlebars_renders_url_and_body()
    {
        var data = new Dictionary<string, object?> { ["baseUrl"] = "https://api.example.com", ["amount"] = 100.5m, ["currency"] = "USD" };

        HandlebarsHelper.Render("{{{baseUrl}}}/invoices", data).Should().Be("https://api.example.com/invoices");
        HandlebarsHelper.Render("""{ "amount": {{{amount}}}, "currency": "{{{currency}}}" }""", data)
            .Should().Be("""{ "amount": 100.5, "currency": "USD" }""");
    }
}
