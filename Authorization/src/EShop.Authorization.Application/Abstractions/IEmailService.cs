namespace EShop.Authorization.Application.Abstractions;

public interface IEmailService
{
    Task SendAsync(EmailMetaData emailMetadata, CancellationToken cancellationToken);
}
