namespace EShop.Finance.Application.Services.IntegrationProvider.Models;

public sealed record CachedToken(string Token, DateTimeOffset? ExpiresAtUtc);
