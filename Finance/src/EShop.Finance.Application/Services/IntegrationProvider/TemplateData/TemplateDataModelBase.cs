using System.Globalization;
using System.Reflection;

namespace EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

public abstract class TemplateDataModelBase : ITemplateDataModel
{
    public string ShortDateFormat { get; protected init; } = "yyyy-MM-dd";

    private readonly Dictionary<string, object?> _templateData = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, object?> GetTemplateDataModel() => _templateData;

    private readonly HashSet<string> _sensitiveKeys = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<string> GetSensitiveKeys() => _sensitiveKeys;

    protected void BuildTemplateData()
    {
        foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(this);

            if (property.GetCustomAttribute<TemplateDataAttribute>() is { } templateDataAttribute)
            {
                AddProperty(property.Name, value, templateDataAttribute);
            }

            if (property.GetCustomAttribute<SensitiveDataAttribute>() is not null)
            {
                AddSensitiveKey(property, value);
            }
        }
    }

    private void AddProperty(string propertyName, object? value, TemplateDataAttribute attribute)
    {
        var key = attribute.PublicName ?? Camelize(propertyName);

        switch (value)
        {
            case null:
                _templateData[key] = null;
                break;

            case ITemplateDataModel model:
                if (attribute.Flatten)
                {
                    foreach (var (nestedKey, nestedValue) in model.GetTemplateDataModel())
                    {
                        _templateData.TryAdd(nestedKey, nestedValue);
                    }
                }
                else
                {
                    _templateData[key] = model.GetTemplateDataModel();
                }

                break;

            case IEnumerable<ITemplateDataModel> models:
                _templateData[key] = models.Select(m => m.GetTemplateDataModel()).ToList();
                break;

            case IDictionary<string, string?> dictionary:
                foreach (var (nestedKey, nestedValue) in dictionary)
                {
                    _templateData[$"{key}.{nestedKey}"] = nestedValue;
                }

                break;

            case string stringValue:
                _templateData[key] = EscapeJson(stringValue.Trim());
                break;

            case DateOnly dateOnly:
                _templateData[key] = dateOnly.ToString(ShortDateFormat, CultureInfo.InvariantCulture);
                break;

            case DateTimeOffset dateTimeOffset:
                ApplyDate(key, dateTimeOffset, dateTimeOffset.ToString(ShortDateFormat, CultureInfo.InvariantCulture), attribute);
                break;

            case DateTime dateTime:
                ApplyDate(key, dateTime, dateTime.ToString(ShortDateFormat, CultureInfo.InvariantCulture), attribute);
                break;

            case bool boolValue:
                _templateData[key] = boolValue;
                _templateData[$"formatted{Pascalize(key)}"] = boolValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
                break;

            default:
                _templateData[key] = value;
                break;
        }
    }

    private void ApplyDate(string key, object rawValue, string formattedValue, TemplateDataAttribute attribute)
    {
        _templateData[key] = attribute.FormatDate ? formattedValue : rawValue;

        if (attribute.FormatDateWithPrefix)
        {
            _templateData[key] = rawValue;
            _templateData[$"formatted{Pascalize(key)}"] = formattedValue;
        }
    }

    private void AddSensitiveKey(PropertyInfo property, object? value)
    {
        var prefix = property.GetCustomAttribute<TemplateDataAttribute>()?.PublicName ?? Camelize(property.Name);

        switch (value)
        {
            case null:
                break;

            case ITemplateDataModel nested:
                foreach (var nestedKey in nested.GetSensitiveKeys())
                {
                    _sensitiveKeys.Add($"{prefix}.{nestedKey}");
                }

                break;

            case IDictionary<string, string?> dictionary:
                foreach (var nestedKey in dictionary.Keys)
                {
                    _sensitiveKeys.Add($"{prefix}.{nestedKey}");
                }

                break;

            default:
                _sensitiveKeys.Add(prefix);
                break;
        }
    }

    private static string EscapeJson(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string Camelize(string value) => value.Length == 0 ? value : char.ToLowerInvariant(value[0]) + value[1..];

    private static string Pascalize(string value) => value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
