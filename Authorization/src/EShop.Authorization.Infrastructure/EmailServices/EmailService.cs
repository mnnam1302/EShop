using EShop.Authorization.Application.Abstractions;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Infrastructure.EmailServices;

internal sealed class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IFluentEmail fluentEmail, ILogger<EmailService> logger)
    {
        _fluentEmail = fluentEmail;
        _logger = logger;
    }

    public async Task Send(EmailMetaData emailMetadata, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Attempting to send email to {EmailToId} with subject: {Subject}", emailMetadata.ToAddress, emailMetadata.Subject);

            var result = await _fluentEmail.To(emailMetadata.ToAddress)
                .Subject(emailMetadata.Subject)
                .Body(emailMetadata.Body)
                .SendAsync(cancellationToken);

            if (!result.Successful)
            {
                _logger.LogWarning("Email sending failed to {EmailToId}. Errors: {Errors}", emailMetadata.ToAddress, result.ErrorMessages);
            }
        }
        catch (System.Net.Mail.SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP Protocol Error while sending email to {EmailToId}. " +
                "StatusCode: {StatusCode}, " +
                "InnerException: {InnerException}",
                emailMetadata.ToAddress,
                smtpEx.StatusCode,
                smtpEx.InnerException?.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while sending email to {EmailToId}", emailMetadata.ToAddress);
            throw;
        }
    }
}
