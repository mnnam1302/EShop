using EShop.Authorization.Application.Abstractions;

namespace EShop.Authorization.Tests.Fakes;

internal sealed class TestEmailService : IEmailService
{
    public Task SendAsync(EmailMetaData emailMetadata, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendUsingTemplateAsync(EmailMetaData emailMetadata, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
