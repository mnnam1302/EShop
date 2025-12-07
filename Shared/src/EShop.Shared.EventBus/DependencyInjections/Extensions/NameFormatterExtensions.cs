using MassTransit;
using System.Reflection;

namespace EShop.Shared.EventBus.DependencyInjections.Extensions;

public static class NameFormatterExtensions
{
    public static string ToKebabCaseString(this MemberInfo member)
    {
        return KebabCaseEndpointNameFormatter.Instance.SanitizeName(member.Name);
    }
}

/// <summary>
/// EntityName is an optional attribute used to override the default entity name for a message type.
/// If present, the entity name will be used when creating the topic or exchange for the message.
/// </summary>
public class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()  => typeof(T).ToKebabCaseString();
}