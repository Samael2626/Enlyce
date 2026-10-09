using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class CommercialTaskConfiguration : IEntityTypeConfiguration<CommercialTask>
{
    public void Configure(EntityTypeBuilder<CommercialTask> builder)
    {
        builder.ToTable("CommercialTasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).ValueGeneratedNever();
        builder.Property(task => task.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(task => task.Title).HasMaxLength(200).IsRequired();
        builder.Property(task => task.Description).HasMaxLength(2_000);
        builder.Property(task => task.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(task => task.DueAt).IsRequired();
        builder.Property(task => task.CreatedAt).IsRequired();
        builder.Property(task => task.UpdatedAt).IsRequired();
        builder.HasOne<Contact>().WithMany().HasForeignKey(task => task.ContactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lead>().WithMany().HasForeignKey(task => task.LeadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(task => task.AdvisorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(task => new { task.AdvisorId, task.DueAt });
        builder.HasIndex(task => new { task.ContactId, task.DueAt });
        builder.HasIndex(task => task.LeadId);
    }
}

public sealed class CommercialTaskEventConfiguration : IEntityTypeConfiguration<CommercialTaskEvent>
{
    public void Configure(EntityTypeBuilder<CommercialTaskEvent> builder)
    {
        builder.ToTable("CommercialTaskEvents");
        builder.HasKey(taskEvent => taskEvent.Id);
        builder.Property(taskEvent => taskEvent.Id).ValueGeneratedNever();
        builder.Property(taskEvent => taskEvent.Action).HasMaxLength(50).IsRequired();
        builder.Property(taskEvent => taskEvent.Comment).HasMaxLength(2_000);
        builder.Property(taskEvent => taskEvent.OccurredAt).IsRequired();
        builder.HasOne<CommercialTask>().WithMany().HasForeignKey(taskEvent => taskEvent.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(taskEvent => taskEvent.ActorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(taskEvent => new { taskEvent.TaskId, taskEvent.OccurredAt });
    }
}
