using YamlDotNet.Serialization;

namespace EShop.Finance.Application.Services.IntegrationProvider.Configuration;

public sealed class FinanceConfiguration
{
    private Dictionary<(string Trigger, string Action), RequestConfiguration> _requestCache = new();

    public string? DateFormat { get; init; } = "yyyy-MM-dd";

    public List<TriggerConfiguration> Triggers { get; init; } = [];
    public List<RequestConfiguration> Requests { get; init; } = [];

    public void InitializeLookupCache()
    {
        var requestsMap = Requests.ToDictionary(r => r.Name, r => r, StringComparer.OrdinalIgnoreCase);
        var cache = new Dictionary<(string Trigger, string Action), RequestConfiguration>();

        foreach (var trigger in Triggers)
        {
            foreach (var action in trigger.Actions)
            {
                if (requestsMap.TryGetValue(action.Request, out var requestConfig))
                {
                    cache[(trigger.Name, action.Name)] = requestConfig;
                }
            }
        }

        _requestCache = cache;
    }

    public RequestConfiguration? GetRequestConfiguration(string triggerName, string actionName)
    {
        return _requestCache.TryGetValue((triggerName, actionName), out var config) ? config : null;
    }
}

public sealed class TriggerConfiguration
{
    public string Name { get; init; } = string.Empty;
    public List<ActionConfiguration> Actions { get; init; } = [];
}

public sealed class ActionConfiguration
{
    public required string Name { get; init; }
    public required string Request { get; init; }
}

public sealed class RequestConfiguration
{
    public required string Name { get; init; }
    public required string UrlTemplate { get; init; }
    public string Method { get; init; } = "POST";
    public string RequestTemplate { get; init; } = string.Empty;
    public string ResponseTemplate { get; init; } = string.Empty;
}
