namespace EShop.Authorization.Infrastructure.DependencyInjections
{
    public sealed class EmailSettingOptions
    {
        public const string SectionName = "EmailSettings";

        public string DefaultFromEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; } = true;
        public int TimeoutMilliseconds { get; set; } = 30000; // 30 seconds default
    }
}
