using Microsoft.Extensions.Configuration;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers
{
    public class CachedRemoteConfiguration
    {
        private readonly IConfiguration configuration;

        public CachedRemoteConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public TimeSpan GetSlidingExpiration()
        {
            // Default cache set to generous 12 hours because we invalidate cache when user permissions change
            var slidingExpirationSetting = configuration.GetSection("CachingService")?.GetValue<string>("SlidingExpiration", "12:00");
            if (!TimeSpan.TryParse(slidingExpirationSetting, out var slidingExpiration))
            {
                throw new InvalidOperationException($"Cannot use 'SlidingExpiration' setting with value '{slidingExpirationSetting}'.");
            }
            return slidingExpiration;
        }
    }
}