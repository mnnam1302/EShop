using EShop.Finance.Application.Services.IntegrationProvider.Authentication;

using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using static EShop.Finance.Application.Services.IntegrationProvider.Configuration.FinanceConfigurationConstants;

namespace EShop.Finance.Application.Services.IntegrationProvider.Generic;

internal sealed class HttpAccountingContext
{
    public FinanceConfiguration Configuration { get; }
    public AuthenticationOptions AuthOptions { get; }

    public HttpAccountingContext(FinanceConfiguration configuration, AuthenticationOptions authOptions)
    {
        Configuration = configuration;
        AuthOptions = authOptions;
    }

    public RequestConfiguration? GetFindPaymentRequest()
        => Configuration.GetRequestConfiguration(Triggers.BookPayment, Actions.Find);

    public RequestConfiguration? GetCreatePaymentRequest()
        => Configuration.GetRequestConfiguration(Triggers.BookPayment, Actions.Create);
}
