namespace Enlyce.Domain.Entities;

public enum EmailDeliveryStatus
{
    AcceptedBySmtp,
    Failed,
    NotConfigured
}

public sealed class EmailDeliveryLog
{
    public Guid Id { get; private set; }
    public string RecipientAddress { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SenderName { get; private set; } = "Sistema";
    public EmailDeliveryStatus Status { get; private set; }
    public string? FailureCode { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    private EmailDeliveryLog() { }

    public static EmailDeliveryLog Create(string recipientAddress, string subject, string? description,
        EmailDeliveryStatus status, string? failureCode, DateTime attemptedAt)
    {
        return new EmailDeliveryLog
        {
            Id = Guid.NewGuid(),
            RecipientAddress = Limit(recipientAddress, 320),
            Subject = Limit(subject, 300),
            Description = Limit(string.IsNullOrWhiteSpace(description) ? "Correo automático" : description, 500),
            SenderName = "Sistema",
            Status = status,
            FailureCode = failureCode,
            AttemptedAt = attemptedAt
        };
    }

    private static string Limit(string value, int length)
    {
        var normalized = value.Trim();
        return normalized.Length <= length ? normalized : normalized[..length];
    }
}
