using EShop.Identity.Domain.Entities;
using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Abstractions.Paging;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Roles;
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

    public string TenentId { get;internal set; }

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
            var command = new Command.CreateRoleCommand(this.Name, string.Empty, string.Empty);

            var result = await _apiContext.PostAsync<Command.CreateRoleCommand>(
                RolesCollectionUri,
                command,
                operationalUser);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Create role error");
            this.Error = ex;
        }
    }

    internal async Task<List<Response.RolesResponse>> GetAllRolesAsync(string? operationUsername = null)
    {
        try
        {
            var operationalUser = _apiContext.GetUserByUsername(operationUsername);
            var query = new Query.GetRoles(null, Paging.Create(1, 50));

            var result = await _apiContext.GetAsync<Query.GetRoles, PagedResult<Response.RolesResponse>>(
                RolesCollectionUri,
                query,
                operationalUser);

            return result.Value.Items;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Get roles error");
            this.Error = ex;
            return new List<Response.RolesResponse>();
        }
    }
}