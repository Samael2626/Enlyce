using Enlyce.Api.Hubs;
using Enlyce.Application.Chat;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.SignalR;

namespace Enlyce.Api.Endpoints.Chat;

public static class ChatModule
{
    public static void MapChat(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").RequireAuthorization();

        group.MapGet("/users", async (
            HttpContext http,
            IAsesorRepository asesorRepository,
            IChatMessageRepository chatRepository,
            CancellationToken ct) =>
        {
            var currentUserId = EndpointAccess.ActorId(http.User);
            if (!currentUserId.HasValue)
                return Results.Unauthorized();

            var asesores = await asesorRepository.ObtenerTodosAsync();
            var unreadMap = await chatRepository.GetUnreadCountByUserAsync(currentUserId.Value, ct);

            var users = asesores
                .Where(a => a.Activo && a.Id != currentUserId.Value)
                .Select(a => new ChatUserDto(
                    a.Id,
                    a.Nombre,
                    a.Correo.Value,
                    a.Rol,
                    unreadMap.GetValueOrDefault(a.Id, 0)))
                .OrderBy(u => u.Name)
                .ToList();

            return Results.Ok(users);
        });

        group.MapGet("/messages", async (
            Guid? recipientId,
            int? limit,
            HttpContext http,
            IAsesorRepository asesorRepository,
            IChatMessageRepository chatRepository,
            CancellationToken ct) =>
        {
            var currentUserId = EndpointAccess.ActorId(http.User);
            if (!currentUserId.HasValue)
                return Results.Unauthorized();

            var messageLimit = Math.Clamp(limit ?? 50, 1, 200);
            IReadOnlyList<ChatMessage> messages;

            if (recipientId.HasValue)
            {
                messages = await chatRepository.GetDirectConversationAsync(
                    currentUserId.Value,
                    recipientId.Value,
                    messageLimit,
                    ct);
            }
            else
            {
                messages = await chatRepository.GetChannelMessagesAsync(messageLimit, ct);
            }

            var asesores = (await asesorRepository.ObtenerTodosAsync())
                .ToDictionary(a => a.Id, a => a.Nombre);

            var dtos = messages.Select(m => new ChatMessageDto(
                m.Id,
                m.SenderId,
                asesores.GetValueOrDefault(m.SenderId, "Usuario"),
                m.RecipientId,
                m.Content,
                m.SentAtUtc,
                m.IsRead,
                m.ReadAtUtc)).ToList();

            return Results.Ok(dtos);
        });

        group.MapPost("/messages", async (
            SendMessageRequest request,
            HttpContext http,
            IAsesorRepository asesorRepository,
            IChatMessageRepository chatRepository,
            IHubContext<ChatHub> hubContext,
            CancellationToken ct) =>
        {
            var currentUserId = EndpointAccess.ActorId(http.User);
            if (!currentUserId.HasValue)
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest(new { error = "El mensaje no puede estar vacio." });

            var trimmed = request.Content.Trim();
            if (trimmed.Length > 2000)
                return Results.BadRequest(new { error = "El mensaje no puede superar 2000 caracteres." });

            var sender = await asesorRepository.ObtenerPorIdAsync(currentUserId.Value);
            if (sender == null || !sender.Activo)
                return Results.Unauthorized();

            if (request.RecipientId.HasValue)
            {
                if (request.RecipientId.Value == currentUserId.Value)
                    return Results.BadRequest(new { error = "No se puede enviar un mensaje a uno mismo." });

                var recipient = await asesorRepository.ObtenerPorIdAsync(request.RecipientId.Value);
                if (recipient == null || !recipient.Activo)
                    return Results.NotFound(new { error = "Destinatario no encontrado o inactivo." });
            }

            var message = ChatMessage.Create(currentUserId.Value, request.RecipientId, trimmed, DateTime.UtcNow);
            await chatRepository.AddAsync(message, ct);

            var dto = new ChatMessageDto(
                message.Id,
                message.SenderId,
                sender.Nombre,
                message.RecipientId,
                message.Content,
                message.SentAtUtc,
                message.IsRead,
                message.ReadAtUtc);

            if (request.RecipientId.HasValue)
            {
                await hubContext.Clients.User(request.RecipientId.Value.ToString()).SendAsync("ReceiveMessage", dto, ct);
                await hubContext.Clients.User(currentUserId.Value.ToString()).SendAsync("ReceiveMessage", dto, ct);
            }
            else
            {
                await hubContext.Clients.Group("GeneralChannel").SendAsync("ReceiveMessage", dto, ct);
            }

            return Results.Ok(dto);
        });

        group.MapPost("/read", async (
            MarkReadRequest request,
            HttpContext http,
            IChatMessageRepository chatRepository,
            IHubContext<ChatHub> hubContext,
            CancellationToken ct) =>
        {
            var currentUserId = EndpointAccess.ActorId(http.User);
            if (!currentUserId.HasValue)
                return Results.Unauthorized();

            var now = DateTime.UtcNow;
            await chatRepository.MarkDirectConversationAsReadAsync(currentUserId.Value, request.SenderId, now, ct);
            await hubContext.Clients.User(request.SenderId.ToString()).SendAsync("MessagesRead", new { ReadBy = currentUserId.Value, ReadAtUtc = now }, ct);

            return Results.NoContent();
        });

        group.MapGet("/unread", async (
            HttpContext http,
            IChatMessageRepository chatRepository,
            CancellationToken ct) =>
        {
            var currentUserId = EndpointAccess.ActorId(http.User);
            if (!currentUserId.HasValue)
                return Results.Unauthorized();

            var totalUnread = await chatRepository.GetUnreadCountAsync(currentUserId.Value, ct);
            var byUser = await chatRepository.GetUnreadCountByUserAsync(currentUserId.Value, ct);

            return Results.Ok(new { Total = totalUnread, ByUser = byUser });
        });
    }
}
