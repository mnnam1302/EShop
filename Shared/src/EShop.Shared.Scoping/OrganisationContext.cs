using EShop.Shared.Contracts.Shared;
using EventFlow.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Scoping;

public class OrganisationContext : ValueObject
{
    private const char PathSeparator = '|';
    private const char Wildcard = '*';
    private static readonly string WildcardSegment = $"{PathSeparator}{Wildcard}";

    public OrganisationContext() { }

    private OrganisationContext(string path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    [MaxLength(ModelConstants.VeryLongText)]
    public string Path { get; private set; } = string.Empty;

    public static OrganisationContext NewRoot(string rootPath) => new OrganisationContext(SanitizePath(rootPath));

    public static OrganisationContext NewChild(string childPath) => NewChild(Parse(childPath));

    public static OrganisationContext NewChild(OrganisationContext parent)
        => parent.AddChild(Guid.NewGuid().ToString());

    public OrganisationContext AddChild(string childIdentifier)
        => new OrganisationContext(string.Join(PathSeparator, this.Path, SanitizePath(childIdentifier)));

    public OrganisationContext AddWildcard() => new(this.Path + WildcardSegment);

    public OrganisationContext RemoveWildcard()
    {
        var pathWithoutWildcard = this.Path.Replace(WildcardSegment, string.Empty, StringComparison.Ordinal);
        return new OrganisationContext(pathWithoutWildcard);
    }

    public static OrganisationContext Empty() => new OrganisationContext(string.Empty);

    public static OrganisationContext Parse(string path) => new OrganisationContext(path ?? string.Empty);

    /// <summary>
    /// Indicates whether this instance represents an organisation context specified by the <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier">The organisation context identifier to verify against.</param>
    /// <returns><c>true</c> if <paramref name="identifier"/> this instance represents an organisation context
    /// specified by the identifier; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <c>null</c>.</exception>
    public bool Represent(string identifier)
    {
        if (identifier is null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        if (this.Path == string.Empty && identifier == string.Empty)
        {
            return true;
        }

        if (identifier == string.Empty)
        {
            return false;
        }

        var pathSegmentToCheck = this.Path.Contains(PathSeparator, StringComparison.Ordinal)
            ? $"{PathSeparator}{identifier}"
            : identifier;

        return this.Path.EndsWith(pathSegmentToCheck, StringComparison.Ordinal);
    }

    public static string SanitizePath(string path) => path.Replace(" ", "-");
}