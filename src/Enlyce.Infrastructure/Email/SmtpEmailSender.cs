using System.Net;
using System.Net.Mail;
using Enlyce.Domain.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enlyce.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public bool EnableSsl { get; init; } = true;
    public string From { get; init; } = "no-reply@lycpropiedadraiz.com";
    public string FromName { get; init; } = "L&C Propiedad Raíz";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}

// Un correo fallido nunca tumba el caso de uso: el lead o la publicacion ya se
// guardaron y perderlos por el SMTP seria peor. El fallo se registra como error
// para que salte en los logs, no se traga en silencio.
public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    : IEmailSender, IEmailAttachmentSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Email SIN CONFIGURAR (Email:Host vacio). No se envio a {To}: {Subject}", to, subject);
            return;
        }

        try
        {
            using var message = new MailMessage(new MailAddress(_options.From, _options.FromName), new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = string.IsNullOrEmpty(_options.Username)
                    ? null
                    : new NetworkCredential(_options.Username, _options.Password),
            };
            await client.SendMailAsync(message);
            logger.LogInformation("Email enviado a {To}: {Subject}", to, subject);
        }
        catch (Exception exception) when (exception is SmtpException or FormatException or InvalidOperationException)
        {
            logger.LogError(exception, "FALLO envio de email a {To}: {Subject}", to, subject);
        }
    }

    public async Task<bool> SendAttachmentAsync(string to, string subject, string body, string fileName,
        string contentType, byte[] content, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Email SIN CONFIGURAR. No se envio el adjunto {FileName}.", fileName);
            return false;
        }

        try
        {
            using var message = new MailMessage(new MailAddress(_options.From, _options.FromName), new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };
            message.Attachments.Add(new Attachment(new MemoryStream(content, writable: false), fileName, contentType));
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = string.IsNullOrEmpty(_options.Username)
                    ? null
                    : new NetworkCredential(_options.Username, _options.Password),
            };
            await client.SendMailAsync(message, ct);
            logger.LogInformation("Email con adjunto enviado: {FileName}.", fileName);
            return true;
        }
        catch (Exception exception) when (exception is SmtpException or FormatException or InvalidOperationException)
        {
            logger.LogError(exception, "Fallo envio de email con adjunto {FileName}.", fileName);
            return false;
        }
    }
}
