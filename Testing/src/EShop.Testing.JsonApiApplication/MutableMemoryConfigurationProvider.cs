using Microsoft.Extensions.Configuration;

namespace EShop.Testing.JsonApiApplication;

public class MutableMemoryConfigurationProvider : ConfigurationProvider, IConfigurationSource
{
    public MutableMemoryConfigurationProvider(IDictionary<string, string> data)
    {
        this.Data = data;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => this;

    public override bool TryGet(string key, out string value)
    {
        return Data.TryGetValue(key, out value);
    }

    public override void Set(string key, string value)
    {
        Data[key] = value;
    }
}
