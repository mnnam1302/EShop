using EShop.Authorization.Tests.Setups;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.CreateRoot;

internal sealed class StepContext(ApiContext apiContext)
{
    internal async Task PublishTenantCreatedAsync(DataTable dataTable, string? username = null)
    {
        var operationalUser = apiContext.GetUserByUsername(username);

        await apiContext.PublishIntegrationEvent<TenantCreated>(new
        {
            TenantId = dataTable.Rows[0]["TenantId"],
            TenantName = dataTable.Rows[0]["TenantName"],
            OwnerUsername = dataTable.Rows[0]["OwnerUsername"],
            OwnerDisplayName = dataTable.Rows[0]["OwnerDisplayName"],
            OwnerEmail = dataTable.Rows[0]["OwnerEmail"],
            ActionUserId = operationalUser.Id,
            ActionUserType = operationalUser.UserType
        });
    }
}
