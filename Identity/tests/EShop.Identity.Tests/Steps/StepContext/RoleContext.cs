using EShop.Identity.Domain.Entities;
using EShop.Identity.Tests.Setups;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EShop.Identity.Tests.Steps.StepContext;

internal class RoleContext
{
    private const string RolesCollectionUri = "api/v1/roles";
    private readonly ApiContext _apiContext;
    private readonly ILogger<RoleContext> _logger;

    public RoleContext(ApiContext apiContext)
    {
        _apiContext = apiContext;
        _logger = apiContext.ServiceProvider.GetRequiredService<ILogger<RoleContext>>();
    }

    public string Name { get; internal set; }

    public string TenentId { get; internal set; }

    public string Description { get; internal set; }

    public Exception Error { get; set; }

    public HttpStatusCode StatusCode { get; internal set; }

    public Permission[] Permissions { get; private set; }

    public List<Role> Roles = new List<Role>();

    public User[] UsersOfRole { get; internal set; }

    internal async Task CreateRoleAsync(string creatorUserName)
    {
        try
        {
            var operationalUser = _apiContext.GetUserByUsername(creatorUserName);
            var result = await _apiContext.Post<Role>(
                RolesCollectionUri,
                new Role(Guid.NewGuid(), this.Name, this.Description),
                operationalUser);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Role error");
            this.Error = ex;
        }
    }
}