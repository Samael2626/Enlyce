using Enlyce.Domain.Ports;

namespace Enlyce.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        // Stub: implementar con SmtpClient o MailKit cuando se necesite envio real.
        // Por ahora solo logea.
        Console.WriteLine($"[EMAIL] To: {to} | Subject: {subject} | Body: {body}");
        return Task.CompletedTask;
    }
}
