using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class LeadAssignmentHistoryConfiguration : IEntityTypeConfiguration<LeadAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<LeadAssignmentHistory> builder)
    {
        builder.ToTable("LeadAssignmentHistory");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Reason).HasMaxLength(500).IsRequired();
        builder.Property(item => item.Source).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(item => item.ChangedAt).IsRequired();
        builder.HasOne<Lead>().WithMany().HasForeignKey(item => item.LeadId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(item => item.PreviousAdvisorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(item => item.NewAdvisorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(item => item.ChangedByAdvisorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.LeadId, item.ChangedAt });
    }
}
