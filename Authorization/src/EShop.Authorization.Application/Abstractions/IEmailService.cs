namespace EShop.Authorization.Application.Abstractions;

public interface IEmailService
{
    Task Send(EmailMetaData emailMetadata, CancellationToken cancellationToken);
}
