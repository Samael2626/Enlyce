using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid? RecipientId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAtUtc { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    private ChatMessage() { }

    public static ChatMessage Create(
        Guid senderId,
        Guid? recipientId,
        string content,
        DateTime sentAtUtc)
    {
        if (senderId == Guid.Empty)
            throw new DomainError("El remitente es obligatorio.");

        if (recipientId == Guid.Empty)
            throw new DomainError("El destinatario no es valido.");

        if (recipientId.HasValue && recipientId.Value == senderId)
            throw new DomainError("No se puede enviar un mensaje a uno mismo.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainError("El mensaje no puede estar vacio.");

        var trimmed = content.Trim();
        if (trimmed.Length > 2000)
            throw new DomainError("El mensaje no puede superar los 2000 caracteres.");

        if (sentAtUtc == default)
            throw new DomainError("La fecha de envio no es valida.");

        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            Content = trimmed,
            SentAtUtc = sentAtUtc,
            IsRead = false,
            ReadAtUtc = null
        };
    }

    public void MarkAsRead(DateTime readAtUtc)
    {
        if (readAtUtc == default)
            throw new DomainError("La fecha de lectura no es valida.");

        if (!IsRead)
        {
            IsRead = true;
            ReadAtUtc = readAtUtc;
        }
    }
}
