using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class LeadDistributionSettingsConfiguration : IEntityTypeConfiguration<LeadDistributionSettings>
{
    public void Configure(EntityTypeBuilder<LeadDistributionSettings> builder)
    {
        builder.ToTable("LeadDistributionSettings");
        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.Rule).HasMaxLength(30).IsRequired();
        builder.Property(settings => settings.RoundRobinCursor).IsRequired();
    }
}
