using MassTransit;
using System.Reflection;

namespace EShop.Shared.EventBus.DependencyInjections.Extensions;

/// <summary>
/// Provides extension methods for formatting names using kebab-case convention.
/// </summary>
public static class NameFormatterExtensions
{
    /// <summary>
    /// Converts a member name to kebab-case format using MassTransit's built-in formatter.
    /// </summary>
    /// <param name="member">The member whose name should be converted.</param>
    /// <returns>A kebab-case formatted string representation of the member name.</returns>
    public static string ToKebabCaseString(this MemberInfo member)
    {
        return KebabCaseEndpointNameFormatter.Instance.SanitizeName(member.Name);
    }
}

/// <summary>
/// Implements IEntityNameFormatter to provide kebab-case formatting for entity names in MassTransit.
/// </summary>
public class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    /// <summary>
    /// Formats an entity name using kebab-case convention.
    /// </summary>
    /// <typeparam name="T">The type to format the name for.</typeparam>
    /// <returns>A kebab-case formatted entity name.</returns>
    public string FormatEntityName<T>() => typeof(T).ToKebabCaseString();
}