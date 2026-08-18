namespace Enlyce.Domain.Ports;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}
