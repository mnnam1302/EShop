using EShop.Shared.Cache.Services;
using FluentAssertions;

namespace EShop.Shared.Cache.Tests;

public sealed class RateLimitRuleResolverTests
{
    private readonly RateLimitRuleResolver _sut = new();

    private static readonly CachedRateLimitPolicy TenantPolicy = new()
    {
        HasPolicy = true,
        Rules =
        [
            new CachedRateLimitRule { Domain = "catalog", Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 50 },
            new CachedRateLimitRule { Domain = CachedRateLimitRule.AllDomains, Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 100 }
        ]
    };

    private static readonly CachedRateLimitPolicy SystemPolicy = new()
    {
        HasPolicy = true,
        Rules =
        [
            new CachedRateLimitRule { Domain = "catalog", Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 10 },
            new CachedRateLimitRule { Domain = CachedRateLimitRule.AllDomains, Scope = RateLimitScopeNames.User, Unit = "Minute", RequestsPerUnit = 20 },
            new CachedRateLimitRule { Domain = "authorization", Scope = RateLimitScopeNames.AnonymousIp, Unit = "Minute", RequestsPerUnit = 5 }
        ]
    };

    [Fact]
    public void Tenant_Specific_Rule_Wins_Over_Everything()
    {
        var rule = _sut.ResolveRule(TenantPolicy, SystemPolicy, "catalog", RateLimitScopeNames.User);

        rule.RequestsPerUnit.Should().Be(50);
    }

    [Fact]
    public void Tenant_Wildcard_Rule_Wins_Over_System_Specific_Rule()
    {
        var rule = _sut.ResolveRule(TenantPolicy, SystemPolicy, "order", RateLimitScopeNames.User);

        rule.RequestsPerUnit.Should().Be(100);
    }

    [Fact]
    public void System_Specific_Rule_Used_When_Tenant_Has_No_Policy()
    {
        var rule = _sut.ResolveRule(null, SystemPolicy, "catalog", RateLimitScopeNames.User);

        rule.RequestsPerUnit.Should().Be(10);
    }

    [Fact]
    public void System_Wildcard_Rule_Used_When_No_More_Specific_Match()
    {
        var rule = _sut.ResolveRule(null, SystemPolicy, "unknown-domain", RateLimitScopeNames.User);

        rule.RequestsPerUnit.Should().Be(20);
    }

    [Fact]
    public void Compiled_Safety_Default_Used_When_Nothing_Matches()
    {
        var rule = _sut.ResolveRule(null, null, "unknown-domain", RateLimitScopeNames.User);

        rule.RequestsPerUnit.Should().Be(60);
    }

    [Fact]
    public void Tenant_Without_Policy_Falls_Through_To_System()
    {
        var tenantWithoutPolicy = new CachedRateLimitPolicy { HasPolicy = false, Rules = [] };

        var rule = _sut.ResolveRule(tenantWithoutPolicy, SystemPolicy, "catalog", RateLimitScopeNames.User);

        rule.RequestsPerUnit.Should().Be(10);
    }

    [Theory]
    [InlineData(RateLimitScopeNames.Tenant, 300)]
    [InlineData(RateLimitScopeNames.User, 60)]
    [InlineData(RateLimitScopeNames.AnonymousIp, 5)]
    public void Compiled_Safety_Default_Provides_A_Value_Per_Scope(string scope, int expectedRequestsPerUnit)
    {
        var rule = _sut.ResolveRule(null, null, "any-domain", scope);

        rule.RequestsPerUnit.Should().Be(expectedRequestsPerUnit);
    }
}
