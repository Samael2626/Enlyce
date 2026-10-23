using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Xunit;

namespace Enlyce.Domain.Tests.Entities;

public class ChatMessageTests
{
    [Fact]
    public void Create_ValidChannelMessage_ShouldInitializeProperties()
    {
        var senderId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var message = ChatMessage.Create(senderId, null, "Hola equipo!", now);

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(senderId, message.SenderId);
        Assert.Null(message.RecipientId);
        Assert.Equal("Hola equipo!", message.Content);
        Assert.Equal(now, message.SentAtUtc);
        Assert.False(message.IsRead);
        Assert.Null(message.ReadAtUtc);
    }

    [Fact]
    public void Create_ValidDirectMessage_ShouldInitializeProperties()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var message = ChatMessage.Create(senderId, recipientId, "Tienes las llaves?", now);

        Assert.Equal(senderId, message.SenderId);
        Assert.Equal(recipientId, message.RecipientId);
        Assert.Equal("Tienes las llaves?", message.Content);
        Assert.False(message.IsRead);
    }

    [Fact]
    public void Create_EmptySender_ShouldThrowDomainError()
    {
        var ex = Assert.Throws<DomainError>(() =>
            ChatMessage.Create(Guid.Empty, null, "Hola", DateTime.UtcNow));
        Assert.Contains("remitente", ex.Message);
    }

    [Fact]
    public void Create_EmptyGuidRecipient_ShouldThrowDomainError()
    {
        var ex = Assert.Throws<DomainError>(() =>
            ChatMessage.Create(Guid.NewGuid(), Guid.Empty, "Hola", DateTime.UtcNow));
        Assert.Contains("destinatario", ex.Message);
    }

    [Fact]
    public void Create_SameSenderAndRecipient_ShouldThrowDomainError()
    {
        var userId = Guid.NewGuid();
        var ex = Assert.Throws<DomainError>(() =>
            ChatMessage.Create(userId, userId, "Hola", DateTime.UtcNow));
        Assert.Contains("uno mismo", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrWhitespaceContent_ShouldThrowDomainError(string? invalidContent)
    {
        var ex = Assert.Throws<DomainError>(() =>
            ChatMessage.Create(Guid.NewGuid(), null, invalidContent!, DateTime.UtcNow));
        Assert.Contains("vacio", ex.Message);
    }

    [Fact]
    public void Create_ContentExceedingMaxLength_ShouldThrowDomainError()
    {
        var longContent = new string('A', 2001);
        var ex = Assert.Throws<DomainError>(() =>
            ChatMessage.Create(Guid.NewGuid(), null, longContent, DateTime.UtcNow));
        Assert.Contains("2000", ex.Message);
    }

    [Fact]
    public void MarkAsRead_UnreadMessage_ShouldSetIsReadAndTimestamp()
    {
        var message = ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), "Mensaje", DateTime.UtcNow);
        var readTime = DateTime.UtcNow.AddMinutes(1);

        message.MarkAsRead(readTime);

        Assert.True(message.IsRead);
        Assert.Equal(readTime, message.ReadAtUtc);
    }

    [Fact]
    public void MarkAsRead_DefaultDate_ShouldThrowDomainError()
    {
        var message = ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), "Mensaje", DateTime.UtcNow);

        var ex = Assert.Throws<DomainError>(() => message.MarkAsRead(default));
        Assert.Contains("fecha de lectura", ex.Message);
    }
}
