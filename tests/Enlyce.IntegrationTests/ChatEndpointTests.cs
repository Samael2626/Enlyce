using Enlyce.Application.Chat;
using Enlyce.Domain.Entities;
using Enlyce.Domain.ValueObjects;
using Enlyce.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Enlyce.IntegrationTests;

public class ChatEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ChatEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/chat/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_Authenticated_ReturnsOtherActiveUsersWithUnreadCount()
    {
        var (client, currentUser) = _factory.CreateAuthenticatedClient("Asesor");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnlyceDbContext>();

        var otherAdvisor = Asesor.Crear(
            "Colega Asesor",
            Email.Create($"colleague-{Guid.NewGuid():N}@test.com"),
            BCrypt.Net.BCrypt.HashPassword("Password123!"),
            "Asesor");
        db.Asesores.Add(otherAdvisor);
        await db.SaveChangesAsync();

        var response = await client.GetAsync("/api/chat/users");
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<ChatUserDto>>();
        Assert.NotNull(users);
        Assert.DoesNotContain(users, u => u.Id == currentUser.Id);
        Assert.Contains(users, u => u.Id == otherAdvisor.Id);
    }

    [Fact]
    public async Task SendMessage_ChannelMessage_PersistsAndReturnsDto()
    {
        var (client, currentUser) = _factory.CreateAuthenticatedClient("Asesor");

        var request = new SendMessageRequest(null, "Reunion general a las 3 PM");
        var response = await client.PostAsJsonAsync("/api/chat/messages", request);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<ChatMessageDto>();
        Assert.NotNull(dto);
        Assert.Equal(currentUser.Id, dto.SenderId);
        Assert.Null(dto.RecipientId);
        Assert.Equal("Reunion general a las 3 PM", dto.Content);
        Assert.False(dto.IsRead);

        // Fetch channel messages
        var historyResponse = await client.GetAsync("/api/chat/messages");
        historyResponse.EnsureSuccessStatusCode();

        var history = await historyResponse.Content.ReadFromJsonAsync<List<ChatMessageDto>>();
        Assert.NotNull(history);
        Assert.Contains(history, m => m.Id == dto.Id);
    }

    [Fact]
    public async Task SendMessage_DirectMessage_PersistsAndCanBeMarkedAsRead()
    {
        var (senderClient, sender) = _factory.CreateAuthenticatedClient("Asesor");
        var (recipientClient, recipient) = _factory.CreateAuthenticatedClient("Asesor");

        // 1. Sender sends direct message to recipient
        var sendRequest = new SendMessageRequest(recipient.Id, "Tienes la documentacion del apartamento?");
        var sendResponse = await senderClient.PostAsJsonAsync("/api/chat/messages", sendRequest);
        sendResponse.EnsureSuccessStatusCode();

        var sentDto = await sendResponse.Content.ReadFromJsonAsync<ChatMessageDto>();
        Assert.NotNull(sentDto);
        Assert.Equal(sender.Id, sentDto.SenderId);
        Assert.Equal(recipient.Id, sentDto.RecipientId);

        // 2. Recipient checks unread count
        var unreadResponse = await recipientClient.GetAsync("/api/chat/unread");
        unreadResponse.EnsureSuccessStatusCode();
        var unreadObj = await unreadResponse.Content.ReadFromJsonAsync<UnreadResponseDto>();
        Assert.NotNull(unreadObj);
        Assert.True(unreadObj.Total >= 1);

        // 3. Recipient reads conversation
        var convoResponse = await recipientClient.GetAsync($"/api/chat/messages?recipientId={sender.Id}");
        convoResponse.EnsureSuccessStatusCode();
        var convo = await convoResponse.Content.ReadFromJsonAsync<List<ChatMessageDto>>();
        Assert.NotNull(convo);
        Assert.Contains(convo, m => m.Id == sentDto.Id);

        // 4. Recipient marks conversation as read
        var markReadResponse = await recipientClient.PostAsJsonAsync("/api/chat/read", new MarkReadRequest(sender.Id));
        Assert.Equal(HttpStatusCode.NoContent, markReadResponse.StatusCode);

        // 5. Unread count should now reflect 0 for this sender
        var unreadAfter = await recipientClient.GetAsync("/api/chat/unread");
        var unreadAfterObj = await unreadAfter.Content.ReadFromJsonAsync<UnreadResponseDto>();
        Assert.NotNull(unreadAfterObj);
        Assert.False(unreadAfterObj.ByUser.ContainsKey(sender.Id) && unreadAfterObj.ByUser[sender.Id] > 0);
    }

    [Fact]
    public async Task SendMessage_ToSelf_ReturnsBadRequest()
    {
        var (client, currentUser) = _factory.CreateAuthenticatedClient("Asesor");

        var request = new SendMessageRequest(currentUser.Id, "Mensaje para mi mismo");
        var response = await client.PostAsJsonAsync("/api/chat/messages", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_EmptyContent_ReturnsBadRequest()
    {
        var (client, _) = _factory.CreateAuthenticatedClient("Asesor");

        var request = new SendMessageRequest(null, "    ");
        var response = await client.PostAsJsonAsync("/api/chat/messages", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record UnreadResponseDto(int Total, Dictionary<Guid, int> ByUser);
}
