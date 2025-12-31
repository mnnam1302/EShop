using EShop.Authorization.Application.Abstractions;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Infrastructure.EmailServices;

internal sealed class EmailService(IFluentEmail fluentEmail, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(EmailMetaData emailMetadata, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Attempting to send email to {EmailToId} with subject: {Subject}", emailMetadata.ToAddress, emailMetadata.Subject);

            var result = await fluentEmail.To(emailMetadata.ToAddress)
                .Subject(emailMetadata.Subject)
                .Body(emailMetadata.Body)
                .SendAsync(cancellationToken);

            if (!result.Successful)
            {
                logger.LogWarning("Email sending failed to {EmailToId}. Errors: {Errors}", emailMetadata.ToAddress, result.ErrorMessages);
            }
        }
        catch (System.Net.Mail.SmtpException smtpEx)
        {
            logger.LogError(smtpEx,
                "SMTP Protocol Error while sending email to {EmailToId}. StatusCode: {StatusCode}, InnerException: {InnerException}",
                emailMetadata.ToAddress, smtpEx.StatusCode, smtpEx.InnerException?.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while sending email to {EmailToId}", emailMetadata.ToAddress);
        }
    }

    public async Task SendUsingTemplateAsync(EmailMetaData emailMetadata, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Attempting to send email to {EmailToId} with subject: {Subject}", emailMetadata.ToAddress, emailMetadata.Subject);

            if (string.IsNullOrEmpty(emailMetadata.Template))
            {
                logger.LogError("Template is null or empty for email to {EmailToId}", emailMetadata.ToAddress);
                throw new InvalidOperationException("Email template cannot be null or empty when using SendUsingTemplateAsync");
            }

            var result = await fluentEmail.To(emailMetadata.ToAddress)
                .Subject(emailMetadata.Subject)
                .UsingTemplate(emailMetadata.Template, emailMetadata.TemplateModel)
                .SendAsync(cancellationToken);

            if (!result.Successful)
            {
                logger.LogWarning("Email sending failed to {EmailToId}. Errors: {Errors}", emailMetadata.ToAddress, result.ErrorMessages);
            }
        }
        catch (System.Net.Mail.SmtpException smtpEx)
        {
            logger.LogError(smtpEx,
                "SMTP Protocol Error while sending email to {EmailToId}. StatusCode: {StatusCode}, InnerException: {InnerException}",
                emailMetadata.ToAddress, smtpEx.StatusCode, smtpEx.InnerException?.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while sending email to {EmailToId}", emailMetadata.ToAddress);
            throw;
        }
    }
}
