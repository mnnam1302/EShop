using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Infrastructure.Options;

public class EmailSettingOptions
{
    public const string SectionName = "EmailSettings";

    [Required]
    [EmailAddress]
    public string DefaultFromEmail { get; set; } = string.Empty;

    [Required]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    [Range(1000, 300000)] // 1 second to 5 minutes
    public int TimeoutMilliseconds { get; set; } = 30000;

    public bool EnableRetry { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public string DisplayName { get; set; } = string.Empty;
}