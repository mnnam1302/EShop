namespace EShop.Shared.Authentication.Abstractions;

/// <summary>
/// Provides functionality to add user-specific context to an <see cref="HttpClient"/> instance.
/// </summary>
/// <remarks>This interface is designed to facilitate the inclusion of user-related information, such as
/// authentication or operational context, into an <see cref="HttpClient"/> for subsequent HTTP requests.</remarks>
public interface ISystemInternalJwtTokenFactory
{
    Task<HttpClient> AddUserContext(HttpClient client, UserData operationalUser, CancellationToken cancellationToken = default);
}
