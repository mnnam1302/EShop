using EShop.Shared.Scoping.DependencyInjections;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers;

public class CachedRemoteConfiguration
{
    private readonly IConfiguration _configuration;

    public CachedRemoteConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TimeSpan GetSlidingExpiration()
    {
        // Default cache set to generous 12 hours because we invalidate cache when user permissions change
        var slidingExpirationSetting = _configuration.GetSection("CachingService")?.GetValue<string>("SlidingExpiration", "12:00");
        if (!TimeSpan.TryParse(slidingExpirationSetting, CultureInfo.InvariantCulture, out TimeSpan slidingExpiration))
        {
            throw new InvalidOperationException($"Cannot use 'SlidingExpiration' setting with value '{slidingExpirationSetting}'.");
        }

        return slidingExpiration;
    }

    public TimeSpan GetSlidingTokenExpiration()
    {
        // Cache set to generous 01 hour because we invalidated cahce when user token change
        var jwtOptions = new JwtOptions();
        _configuration.GetSection(nameof(JwtOptions)).Bind(jwtOptions);

        var slidingTokenExpirationSetting = jwtOptions.AccessTokenExpiryMinutes.ToString();

        if (!TimeSpan.TryParse(slidingTokenExpirationSetting, CultureInfo.InvariantCulture, out TimeSpan slidingExpiration))
        {
            throw new InvalidOperationException($"Cannot use 'SlidingTokenExpiration' setting with value '{slidingTokenExpirationSetting}'.");
        }

        return slidingExpiration;
    }
}