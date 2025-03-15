using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace EShop.Shared.DomainTools;

public interface IResiliencePolicyFactory
{
    AsyncPolicy CreateDbUpdateHandlingAsyncPolly(ILogger logger);
}

public class ResiliencePolicyFactory : IResiliencePolicyFactory
{
    private const int BaseNumber = 30;
    private const int JitterMultiplierWindow = 30;

    public AsyncPolicy CreateDbUpdateHandlingAsyncPolly(ILogger logger)
    {
        int retryCount = 5;
        return Policy
            .Handle<DbUpdateException>()
            .WaitAndRetryAsync(retryCount, retryAttemp => GetExponentialBackOffPlusSomeJitter(retryAttemp));
    }

    private static TimeSpan GetExponentialBackOffPlusSomeJitter(int retryAttempt)
    {
        return TimeSpan.FromSeconds(Math.Pow(BaseNumber, retryAttempt)) + Scoping.Jitterer.GetJitteredDelay() * JitterMultiplierWindow;
    }
}