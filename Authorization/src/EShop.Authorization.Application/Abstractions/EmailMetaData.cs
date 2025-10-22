namespace EShop.Authorization.Application.Abstractions;

public sealed class EmailMetaData
{
    public string ToAddress { get; set; }
    public string Subject { get; set; }
    public string? Body { get; set; }
    public string? AttachmentPath { get; set; }

    public EmailMetaData(string toAddress, string subject, string? body = "", string? attachmentPath = "")
    {
        ToAddress = toAddress;
        Subject = subject;
        Body = body;
        AttachmentPath = attachmentPath;
    }
}
