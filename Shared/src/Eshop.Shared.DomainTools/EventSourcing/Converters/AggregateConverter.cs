using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using JsonNet.ContractResolvers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace EShop.Shared.DomainTools.EventSourcing.Converters;

public sealed class AggregateConverter : ValueConverter<IAggregate?, string>
{
    public AggregateConverter()
        : base(
            @event => JsonConvert.SerializeObject(@event, typeof(IAggregate), SerializerSettings()),
            jsonString => JsonConvert.DeserializeObject<IAggregate>(jsonString, DeserializerSettings()))
    { }

    private static JsonSerializerSettings SerializerSettings()
    {
        JsonSerializerSettings jsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        jsonSerializerSettings.Converters.Add(new DateOnlyJsonConverter());
        jsonSerializerSettings.Converters.Add(new ExpirationDateOnlyJsonConverter());

        return jsonSerializerSettings;
    }

    private static JsonSerializerSettings DeserializerSettings()
    {
        JsonSerializerSettings jsonDeserializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new PrivateSetterContractResolver()
        };

        jsonDeserializerSettings.Converters.Add(new DateOnlyJsonConverter());
        jsonDeserializerSettings.Converters.Add(new ExpirationDateOnlyJsonConverter());

        return jsonDeserializerSettings;
    }
}
