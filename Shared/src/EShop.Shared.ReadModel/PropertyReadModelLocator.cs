using System.Collections.Concurrent;
using System.Reflection;

namespace EShop.Shared.ReadModel;

/// <summary>
/// Convention-based locator that extracts the read model ID from a named property on the event.
/// Uses reflection with caching for performance.
/// </summary>
/// <typeparam name="TReadModel">The read model type this locator serves.</typeparam>
public sealed class PropertyReadModelLocator<TReadModel> : IReadModelLocator<TReadModel>
    where TReadModel : class, IReadModel
{
    private readonly string _propertyName;
    private readonly ConcurrentDictionary<Type, PropertyInfo> _cache = new();

    public PropertyReadModelLocator(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        _propertyName = propertyName;
    }

    public string GetReadModelId(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = @event.GetType();

        var property = _cache.GetOrAdd(eventType, type =>
            type.GetProperty(_propertyName)
            ?? throw new InvalidOperationException($"Event '{type.Name}' does not have property '{_propertyName}'."));

        var value = property.GetValue(@event)
            ?? throw new InvalidOperationException($"Property '{_propertyName}' on event '{eventType.Name}' returned null.");

        return value.ToString()!;
    }
}