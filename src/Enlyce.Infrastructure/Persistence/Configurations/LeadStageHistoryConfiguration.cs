using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class LeadStageHistoryConfiguration : IEntityTypeConfiguration<LeadStageHistory>
{
    public void Configure(EntityTypeBuilder<LeadStageHistory> builder)
    {
        builder.ToTable("LeadStageHistory");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.PreviousStage).HasMaxLength(80).IsRequired();
        builder.Property(item => item.NewStage).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Reason).HasMaxLength(500).IsRequired();
        builder.Property(item => item.ChangedAt).IsRequired();
        builder.HasOne<Lead>().WithMany().HasForeignKey(item => item.LeadId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(item => item.ActorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.LeadId, item.ChangedAt });
    }
}
