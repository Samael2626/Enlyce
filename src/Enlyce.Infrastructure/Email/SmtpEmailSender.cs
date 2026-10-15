using System.Net;
using System.Net.Mail;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailSender> logger,
    IServiceScopeFactory scopeFactory)
    : IEmailSender, IEmailAttachmentSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body, string? description = null)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Email SIN CONFIGURAR (Email:Host vacio). No se envio a {To}: {Subject}", to, subject);
            await SaveAttemptAsync(to, subject, description, EmailDeliveryStatus.NotConfigured, null);
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
            await SaveAttemptAsync(to, subject, description, EmailDeliveryStatus.AcceptedBySmtp, null);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "FALLO envio de email a {To}: {Subject}", to, subject);
            await SaveAttemptAsync(to, subject, description, EmailDeliveryStatus.Failed, exception.GetType().Name);
        }
    }

    public async Task<bool> SendAttachmentAsync(string to, string subject, string body, string fileName,
        string contentType, byte[] content, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Email SIN CONFIGURAR. No se envio el adjunto {FileName}.", fileName);
            await SaveAttemptAsync(to, subject, $"Adjunto: {fileName}", EmailDeliveryStatus.NotConfigured, null);
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
            await SaveAttemptAsync(to, subject, $"Adjunto: {fileName}", EmailDeliveryStatus.AcceptedBySmtp, null);
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Fallo envio de email con adjunto {FileName}.", fileName);
            await SaveAttemptAsync(to, subject, $"Adjunto: {fileName}", EmailDeliveryStatus.Failed, exception.GetType().Name);
            return false;
        }
    }

    private async Task SaveAttemptAsync(string to, string subject, string? description,
        EmailDeliveryStatus status, string? failureCode)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();
            context.EmailDeliveryLogs.Add(EmailDeliveryLog.Create(
                to, subject, description, status, failureCode, DateTime.UtcNow));
            await context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "No se pudo guardar el registro de envio de correo.");
        }
    }
}
