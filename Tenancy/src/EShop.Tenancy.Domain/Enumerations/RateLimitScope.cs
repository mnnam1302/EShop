namespace EShop.Tenancy.Domain.Enumerations;

public enum RateLimitScope
{
    Tenant = 1,
    User = 2,
    AnonymousIp = 3,
}
