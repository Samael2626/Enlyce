using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IChatMessageRepository
{
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetDirectConversationAsync(Guid userId1, Guid userId2, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetChannelMessagesAsync(int limit = 50, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid recipientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, int>> GetUnreadCountByUserAsync(Guid recipientId, CancellationToken cancellationToken = default);
    Task MarkDirectConversationAsReadAsync(Guid recipientId, Guid senderId, DateTime readAtUtc, CancellationToken cancellationToken = default);
}
