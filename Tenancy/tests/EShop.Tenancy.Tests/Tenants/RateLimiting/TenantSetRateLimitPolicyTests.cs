using EShop.Shared.DomainTools.Exceptions;
using EShop.Tenancy.Domain.Commands;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Enumerations;
using FluentAssertions;
using Xunit;

namespace EShop.Tenancy.Tests.Tenants.RateLimiting;

public sealed class TenantSetRateLimitPolicyTests
{
    [Fact]
    public void SetRateLimitPolicy_With_Valid_Policy_Updates_TenantSetting()
    {
        var tenant = CreateTenantWithSettings();
        var policy = new RateLimitPolicy(
        [
            new RateLimitRule { Domain = "*", Scope = nameof(RateLimitScope.User), Unit = nameof(RateLimitUnit.Minute), RequestsPerUnit = 120, Burst = 150 }
        ]);

        tenant.SetRateLimitPolicy(policy);

        tenant.TenantSettings.Single().RateLimitPolicy.Should().BeSameAs(policy);
    }

    [Fact]
    public void SetRateLimitPolicy_With_Invalid_Policy_Throws_And_Leaves_Setting_Unchanged()
    {
        var tenant = CreateTenantWithSettings();
        var invalidPolicy = new RateLimitPolicy(
        [
            new RateLimitRule { Domain = "*", Scope = nameof(RateLimitScope.User), Unit = nameof(RateLimitUnit.Minute), RequestsPerUnit = 0 }
        ]);

        var act = () => tenant.SetRateLimitPolicy(invalidPolicy);

        act.Should().Throw<DomainException>();
        tenant.TenantSettings.Single().RateLimitPolicy.Should().BeNull();
    }

    [Fact]
    public void New_Tenant_Has_No_Seeded_RateLimitPolicy()
    {
        var tenant = CreateTenantWithSettings();

        tenant.TenantSettings.Single().RateLimitPolicy.Should().BeNull();
    }

    [Fact]
    public void SetRateLimitPolicy_Without_TenantSetting_Throws()
    {
        var tenant = Tenant.Create(new CreateTenantCommand
        {
            Id = "acme",
            Name = "Acme",
            OwnerUsername = "owner",
            OwnerEmail = "owner@acme.com"
        });

        var policy = new RateLimitPolicy([
            new RateLimitRule { Domain = "*", Scope = nameof(RateLimitScope.User), Unit = nameof(RateLimitUnit.Minute), RequestsPerUnit = 10 }
        ]);

        var act = () => tenant.SetRateLimitPolicy(policy);

        act.Should().Throw<BadRequestException>();
    }

    private static Tenant CreateTenantWithSettings()
    {
        var tenant = Tenant.Create(new CreateTenantCommand
        {
            Id = "acme",
            Name = "Acme",
            OwnerUsername = "owner",
            OwnerEmail = "owner@acme.com"
        });

        tenant.AddDefaultTenantSetting();

        return tenant;
    }
}
