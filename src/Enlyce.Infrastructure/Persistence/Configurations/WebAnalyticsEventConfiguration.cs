using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class WebAnalyticsEventConfiguration : IEntityTypeConfiguration<WebAnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<WebAnalyticsEvent> builder)
    {
        builder.ToTable("WebAnalyticsEvents");
        builder.HasKey(analyticsEvent => analyticsEvent.Id);
        builder.Property(analyticsEvent => analyticsEvent.Id).ValueGeneratedNever();
        builder.Property(analyticsEvent => analyticsEvent.SessionHash).HasMaxLength(64).IsRequired();
        builder.Property(analyticsEvent => analyticsEvent.EventType).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(analyticsEvent => analyticsEvent.Path).HasMaxLength(200).IsRequired();
        builder.Property(analyticsEvent => analyticsEvent.PropertySlug).HasMaxLength(160);
        builder.Property(analyticsEvent => analyticsEvent.OccurredAt).IsRequired();
        builder.HasIndex(analyticsEvent => analyticsEvent.OccurredAt);
        builder.HasIndex(analyticsEvent => new { analyticsEvent.EventType, analyticsEvent.OccurredAt });
    }
}
