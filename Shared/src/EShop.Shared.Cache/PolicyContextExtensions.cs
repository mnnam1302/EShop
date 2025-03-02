using Microsoft.Extensions.Logging;
using Polly;

namespace EShop.Shared.Cache;

internal static class PolicyContextExtensions
{
    public static bool TryGetLogger(this Context context, out ILogger? logger)
    {
        if (context.TryGetValue(PolicyContextItems.Logger, out var loggerObject) && loggerObject is ILogger foundLogger)
        {
            logger = foundLogger;
            return true;
        }

        logger = null;
        return false;
    }
}