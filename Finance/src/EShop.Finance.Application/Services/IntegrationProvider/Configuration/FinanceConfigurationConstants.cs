namespace EShop.Finance.Application.Services.IntegrationProvider.Configuration;

public static class FinanceConfigurationConstants
{
    public static class Triggers
    {
        public const string BookPayment = "BookPayment";
        public const string RefundPayment = "RefundPayment";
    }

    public static class Actions
    {
        public const string Find = "Find";
        public const string Create = "Create";
    }
}
