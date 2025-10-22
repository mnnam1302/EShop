using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace EShop.Shared.DomainTools;

public interface IResiliencePolicyFactory
{
    Policy CreateConcurrentUpdateHandlingPolicy(ILogger logger);
    AsyncPolicy CreateDbUpdateHandlingAsyncPolly(ILogger logger);
}

public class ResiliencePolicyFactory : IResiliencePolicyFactory
{
    private const int BaseNumber = 30;
    private const int JitterMultiplierWindow = 30;

    public Policy CreateConcurrentUpdateHandlingPolicy(ILogger logger)
    {
        int retryCount = 5;
        return Policy
            .Handle<DbUpdateException>()
            .WaitAndRetry(retryCount, retryAttempt =>
                GetExponentialBackOffPlusSomeJitter(retryAttempt),
                (exception, timeSpan, retryAttempt, context) =>
                {
                    logger.LogDebug("DBUpdateException handled, Retry number {current}/{max} for exception '{exception}'", retryAttempt, retryCount, exception.Message);
                });
    }

    public AsyncPolicy CreateDbUpdateHandlingAsyncPolly(ILogger logger)
    {
        int retryCount = 5;
        return Policy
            .Handle<DbUpdateException>()
            .WaitAndRetryAsync(retryCount, retryAttempt => GetExponentialBackOffPlusSomeJitter(retryAttempt));
    }

    private static TimeSpan GetExponentialBackOffPlusSomeJitter(int retryAttempt)
    {
        return TimeSpan.FromSeconds(Math.Pow(BaseNumber, retryAttempt)) + Jitterer.GetJitteredDelay() * JitterMultiplierWindow;
    }
}