using Newtonsoft.Json;

namespace EShop.Shared.EventBus.JsonConverters;

public class TypeNameHandlingConverter : JsonConverter
{
    private readonly TypeNameHandling _typeNameHandling;

    public TypeNameHandlingConverter(TypeNameHandling typeNameHandling)
    {
        _typeNameHandling = typeNameHandling;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        => new JsonSerializer { TypeNameHandling = _typeNameHandling }.Deserialize(reader, objectType);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => new JsonSerializer { TypeNameHandling = _typeNameHandling }.Serialize(writer, value);

    public override bool CanConvert(Type type) => IsMassTransitOrSystemType(type) is false;

    private static bool IsMassTransitOrSystemType(Type type)
    {
        return (type.Assembly.FullName?.Contains(nameof(MassTransit)) ?? false) ||
            type.Assembly.IsDynamic ||
            type.Assembly == typeof(object).Assembly;
    }
}