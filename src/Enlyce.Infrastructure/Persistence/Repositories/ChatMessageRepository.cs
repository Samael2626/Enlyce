using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly EnlyceDbContext _db;

    public ChatMessageRepository(EnlyceDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetDirectConversationAsync(
        Guid userId1,
        Guid userId2,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = await _db.ChatMessages
            .AsNoTracking()
            .Where(m =>
                (m.SenderId == userId1 && m.RecipientId == userId2) ||
                (m.SenderId == userId2 && m.RecipientId == userId1))
            .OrderByDescending(m => m.SentAtUtc)
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync(cancellationToken);

        messages.Reverse();
        return messages;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetChannelMessagesAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.RecipientId == null)
            .OrderByDescending(m => m.SentAtUtc)
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync(cancellationToken);

        messages.Reverse();
        return messages;
    }

    public async Task<int> GetUnreadCountAsync(Guid recipientId, CancellationToken cancellationToken = default)
    {
        return await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.RecipientId == recipientId && !m.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetUnreadCountByUserAsync(
        Guid recipientId,
        CancellationToken cancellationToken = default)
    {
        var unread = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.RecipientId == recipientId && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SenderId, x => x.Count, cancellationToken);

        return unread;
    }

    public async Task MarkDirectConversationAsReadAsync(
        Guid recipientId,
        Guid senderId,
        DateTime readAtUtc,
        CancellationToken cancellationToken = default)
    {
        var unreadMessages = await _db.ChatMessages
            .Where(m => m.RecipientId == recipientId && m.SenderId == senderId && !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var message in unreadMessages)
        {
            message.MarkAsRead(readAtUtc);
        }

        if (unreadMessages.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
