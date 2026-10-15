using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class CustomerFunnelStageConfiguration : IEntityTypeConfiguration<CustomerFunnelStage>
{
    public void Configure(EntityTypeBuilder<CustomerFunnelStage> builder)
    {
        builder.ToTable("CustomerFunnelStages");
        builder.HasKey(stage => stage.Id);
        builder.Property(stage => stage.Id).ValueGeneratedNever();
        builder.Property(stage => stage.FunnelName).HasMaxLength(100).IsRequired();
        builder.Property(stage => stage.Name).HasMaxLength(100).IsRequired();
        builder.Property(stage => stage.SortOrder).IsRequired();
        builder.Property(stage => stage.IsDefaultForInbound).IsRequired();
        builder.Property(stage => stage.CreatedAtUtc).IsRequired();
        builder.HasIndex(stage => new { stage.FunnelName, stage.Name }).IsUnique();
        builder.HasIndex(stage => stage.SortOrder);
        builder.HasIndex(stage => stage.IsDefaultForInbound).IsUnique()
            .HasFilter("\"IsDefaultForInbound\" = TRUE");
    }
}
