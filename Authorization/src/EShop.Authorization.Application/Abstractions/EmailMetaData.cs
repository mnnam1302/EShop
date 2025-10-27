namespace EShop.Authorization.Application.Abstractions;

public sealed class EmailMetaData
{
    public EmailMetaData(string toAddress, string subject, string? body = "")
    {
        ToAddress = toAddress;
        Subject = subject;
        Body = body;
    }

    public string ToAddress { get; set; }
    public string Subject { get; set; }
    public string? Body { get; set; }
    public string? AttachmentPath { get; set; }
    public string? FromAddress { get; set; }
    public string? FromDisplayName { get; set; }
    public bool IsHtml { get; set; } = false;
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;

    public string? TemplateName { get; set; }
    public string? Template { get; set; }
    public object? TemplateModel { get; set; }

    public List<string> CcAddresses { get; set; } = new();
    public List<string> BccAddresses { get; set; } = new();

    public List<EmailAttachment> Attachments { get; set; } = new();
}

public class EmailAttachment
{
    public string FilePath { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ContentType { get; set; }
}

public enum EmailPriority
{
    Low,
    Normal,
    High
}

public static class EmailMetaDataFactory
{
    public static EmailMetaData CreateWelcomeEmail(string toAddress, string userName, string rawPassword)
    {
        var template = @"
            <h1>Welcome to EShop, {{ @Model.UserName }}!</h1>
            <p>Your account has been created successfully.</p>
            <p>Your temporary password is: <strong>{{ @Model.DefaultPassword }}</strong></p>
            <p>Please change your password after your first login.</p>
            <p>Happy shopping!</p>";

        return new EmailMetaData(toAddress, "Welcome to EShop!")
        {
            TemplateName = "welcome",
            Template = template,
            TemplateModel = new { UserName = userName, DefaultPassword = rawPassword },
            Body = $"Welcome to EShop, {userName}! Your temporary password is: {rawPassword}",
            IsHtml = true
        };
    }

    public static EmailMetaData CreatePasswordResetEmail(string toAddress, string resetToken)
    {
        return new EmailMetaData(toAddress, "Password Reset Request")
        {
            TemplateName = "password-reset",
            TemplateModel = new { ResetToken = resetToken, ExpiryHours = 24 },
            IsHtml = true,
            Priority = EmailPriority.High
        };
    }
}