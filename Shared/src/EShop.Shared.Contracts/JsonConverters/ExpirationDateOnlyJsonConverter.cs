using Newtonsoft.Json;
using System.Globalization;

namespace EShop.Shared.Contracts.JsonConverters;

public class ExpirationDateOnlyJsonConverter : JsonConverter
{
    private const string Format = "MM/yy";


    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return DateOnly.ParseExact(reader.Value as string ?? string.Empty, Format, CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue((value is DateOnly date ? date : default).ToString(Format, CultureInfo.InvariantCulture));
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateOnly);
    }
}