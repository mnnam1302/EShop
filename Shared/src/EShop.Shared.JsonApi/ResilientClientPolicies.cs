using EShop.Shared.DomainTools;
using EventFlow.Exceptions;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace EShop.Shared.JsonApi;

public static class ResilientClientPolicies
{
    private const int _durationOfBreakInSeconds = 30;
    private const int _handledEventsAllowedBeforeBreaking = 3;

    public static IAsyncPolicy<HttpResponseMessage> GetRetryOnBadGatewayPolicy(int retryCount = 3)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == HttpStatusCode.BadGateway)
            .WaitAndRetryAsync(retryCount, retryAttempt => GetExponentialBackOffPlusSomeJitter(retryAttempt));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryOnErrorPolicy(int retryCount = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => GetExponentialBackOffPlusSomeJitter(retryAttempt));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryOnErrorAndNotFoundPolicy(int retryCount = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(retryCount, retryAttempt => GetExponentialBackOffPlusSomeJitter(retryAttempt));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryOnToManyRequestPolicy(int retryCount = 3)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == HttpStatusCode.TooManyRequests && msg.Headers.RetryAfter != null)
            .WaitAndRetryAsync(
                retryCount,
                (_, result, _) => result.Result.Headers.RetryAfter?.Delta == null
                    ? TimeSpan.FromSeconds(10)
                    : result.Result.Headers.RetryAfter.Delta.Value,
                (_, _, _, _) => throw DomainError.With("Request failed after retries"));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(_handledEventsAllowedBeforeBreaking, TimeSpan.FromSeconds(_durationOfBreakInSeconds));
    }

    private static TimeSpan GetExponentialBackOffPlusSomeJitter(int retryAttempt)
    {
        const int BaseNumber = 2;
        return TimeSpan.FromSeconds(Math.Pow(BaseNumber, retryAttempt)) + Jitterer.GetJitteredDelay();
    }
}
