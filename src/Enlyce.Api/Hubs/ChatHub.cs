using Enlyce.Application.Chat;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Enlyce.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatMessageRepository _chatRepository;
    private readonly IAsesorRepository _asesores;

    public ChatHub(IChatMessageRepository chatRepository, IAsesorRepository asesores)
    {
        _chatRepository = chatRepository;
        _asesores = asesores;
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GeneralChannel");
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(Guid? recipientId, string content)
    {
        var senderId = GetCurrentUserId();
        if (!senderId.HasValue)
            throw new HubException("Usuario no autenticado.");

        var sender = await _asesores.ObtenerPorIdAsync(senderId.Value);
        if (sender == null || !sender.Activo)
            throw new HubException("Remitente no valido.");

        if (recipientId.HasValue)
        {
            var recipient = await _asesores.ObtenerPorIdAsync(recipientId.Value);
            if (recipient == null || !recipient.Activo)
                throw new HubException("Destinatario no encontrado.");
        }

        var message = ChatMessage.Create(senderId.Value, recipientId, content, DateTime.UtcNow);
        await _chatRepository.AddAsync(message);

        var dto = new ChatMessageDto(
            message.Id,
            message.SenderId,
            sender.Nombre,
            message.RecipientId,
            message.Content,
            message.SentAtUtc,
            message.IsRead,
            message.ReadAtUtc);

        if (recipientId.HasValue)
        {
            await Clients.User(recipientId.Value.ToString()).SendAsync("ReceiveMessage", dto);
            await Clients.User(senderId.Value.ToString()).SendAsync("ReceiveMessage", dto);
        }
        else
        {
            await Clients.Group("GeneralChannel").SendAsync("ReceiveMessage", dto);
        }
    }

    public async Task MarkAsRead(Guid senderId)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
            return;

        var now = DateTime.UtcNow;
        await _chatRepository.MarkDirectConversationAsReadAsync(currentUserId.Value, senderId, now);
        await Clients.User(senderId.ToString()).SendAsync("MessagesRead", new { ReadBy = currentUserId.Value, ReadAtUtc = now });
    }

    public async Task SendTyping(Guid? recipientId)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
            return;

        if (recipientId.HasValue)
        {
            await Clients.User(recipientId.Value.ToString()).SendAsync("UserTyping", new { SenderId = currentUserId.Value });
        }
        else
        {
            await Clients.OthersInGroup("GeneralChannel").SendAsync("UserTyping", new { SenderId = currentUserId.Value, InChannel = true });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
