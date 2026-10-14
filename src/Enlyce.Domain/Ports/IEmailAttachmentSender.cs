namespace Enlyce.Domain.Ports;

public interface IEmailAttachmentSender
{
    Task<bool> SendAttachmentAsync(string to, string subject, string body, string fileName,
        string contentType, byte[] content, CancellationToken ct = default);
}
