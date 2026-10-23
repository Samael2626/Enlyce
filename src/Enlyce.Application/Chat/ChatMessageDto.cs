namespace Enlyce.Application.Chat;

public record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    Guid? RecipientId,
    string Content,
    DateTime SentAtUtc,
    bool IsRead,
    DateTime? ReadAtUtc);

public record ChatUserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    int UnreadCount);

public record SendMessageRequest(
    Guid? RecipientId,
    string Content);

public record MarkReadRequest(
    Guid SenderId);
