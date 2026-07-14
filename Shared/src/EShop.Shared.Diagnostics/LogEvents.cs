using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.Diagnostics
{
    public static class LogEvents
    {
        public static readonly EventId TenantCreationFailed = new EventId(1, nameof(TenantCreationFailed));
        public static readonly EventId GetTenantsFailed = new EventId(2, nameof(GetTenantsFailed));

        public static readonly EventId RedisCircuitBreakerActivated = new EventId(55, nameof(RedisCircuitBreakerActivated));
        public static readonly EventId RateLimiterFailOpen = new EventId(56, nameof(RateLimiterFailOpen));
    }
}
