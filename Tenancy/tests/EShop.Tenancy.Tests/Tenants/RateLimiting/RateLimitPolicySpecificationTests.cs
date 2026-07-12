using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Enumerations;
using EShop.Tenancy.Domain.Specifications;
using FluentAssertions;
using Xunit;

namespace EShop.Tenancy.Tests.Tenants.RateLimiting;

public sealed class RateLimitPolicySpecificationTests
{
    [Fact]
    public void Policy_With_Valid_Rules_Is_Satisfied()
    {
        var policy = new RateLimitPolicy(
        [
            new RateLimitRule { Domain = "*", Scope = RateLimitScope.User, Unit = RateLimitUnit.Minute, RequestsPerUnit = 120, Burst = 150 },
            new RateLimitRule { Domain = "authorization", Scope = RateLimitScope.AnonymousIp, Unit = RateLimitUnit.Minute, RequestsPerUnit = 5 }
        ]);

        var result = RateLimitPolicySpecification.New().IsSatisfiedBy(policy);

        result.Should().BeTrue();
    }

    [Fact]
    public void Policy_With_NonPositive_RequestsPerUnit_Is_Rejected()
    {
        var policy = new RateLimitPolicy(
        [
            new RateLimitRule { Domain = "*", Scope = RateLimitScope.User, Unit = RateLimitUnit.Minute, RequestsPerUnit = 0 }
        ]);

        var reasons = RateLimitPolicySpecification.New().WhyIsNotSatisfiedBy(policy);

        reasons.Should().ContainMatch("*non-positive requestsPerUnit*");
    }

    [Fact]
    public void Policy_With_Burst_Below_RequestsPerUnit_Is_Rejected()
    {
        var policy = new RateLimitPolicy(
        [
            new RateLimitRule { Domain = "*", Scope = RateLimitScope.User, Unit = RateLimitUnit.Minute, RequestsPerUnit = 100, Burst = 5 }
        ]);

        var reasons = RateLimitPolicySpecification.New().WhyIsNotSatisfiedBy(policy);

        reasons.Should().ContainMatch("*burst*below requestsPerUnit*");
    }

    [Fact]
    public void Policy_With_Duplicate_Domain_And_Scope_Is_Rejected()
    {
        var policy = new RateLimitPolicy(
        [
            new RateLimitRule { Domain = "catalog", Scope = RateLimitScope.Tenant, Unit = RateLimitUnit.Minute, RequestsPerUnit = 100 },
            new RateLimitRule { Domain = "catalog", Scope = RateLimitScope.Tenant, Unit = RateLimitUnit.Hour, RequestsPerUnit = 1000 }
        ]);

        var reasons = RateLimitPolicySpecification.New().WhyIsNotSatisfiedBy(policy);

        reasons.Should().ContainMatch("*duplicate rule*catalog*Tenant*");
    }

    [Fact]
    public void Policy_Exceeding_Max_Rules_Is_Rejected()
    {
        var rules = Enumerable.Range(0, RateLimitPolicySpecification.MaxRules + 1)
            .Select(i => new RateLimitRule { Domain = $"domain-{i}", Scope = RateLimitScope.Tenant, Unit = RateLimitUnit.Minute, RequestsPerUnit = 10 });

        var policy = new RateLimitPolicy(rules);

        var reasons = RateLimitPolicySpecification.New().WhyIsNotSatisfiedBy(policy);

        reasons.Should().ContainMatch("*exceeds the maximum*");
    }
}
