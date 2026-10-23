using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.SenderId)
            .IsRequired();

        builder.Property(m => m.RecipientId)
            .IsRequired(false);

        builder.Property(m => m.Content)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.SentAtUtc)
            .IsRequired();

        builder.Property(m => m.IsRead)
            .IsRequired();

        builder.Property(m => m.ReadAtUtc)
            .IsRequired(false);

        builder.HasIndex(m => new { m.SenderId, m.RecipientId, m.SentAtUtc });
        builder.HasIndex(m => new { m.RecipientId, m.IsRead });
        builder.HasIndex(m => m.SentAtUtc);
    }
}
