using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class LeadSlaRuleConfiguration : IEntityTypeConfiguration<LeadSlaRule>
{
    public void Configure(EntityTypeBuilder<LeadSlaRule> builder)
    {
        builder.ToTable("LeadSlaRules");
        builder.HasKey(rule => rule.Id);
        builder.Property(rule => rule.Id).ValueGeneratedNever();
        builder.Property(rule => rule.SourceKey).HasMaxLength(100).IsRequired();
        builder.Property(rule => rule.OperationType).HasMaxLength(10).IsRequired();
        builder.Property(rule => rule.FirstResponseMinutes).IsRequired();
        builder.Property(rule => rule.InactivityDays);
        builder.Property(rule => rule.Enabled).IsRequired();
        builder.Property(rule => rule.UpdatedAtUtc).IsRequired();
        builder.HasIndex(rule => new { rule.SourceKey, rule.OperationType }).IsUnique();
    }
}
