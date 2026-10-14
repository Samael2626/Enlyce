using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class ContactEmailPermissionEventConfiguration : IEntityTypeConfiguration<ContactEmailPermissionEvent>
{
    public void Configure(EntityTypeBuilder<ContactEmailPermissionEvent> builder)
    {
        builder.ToTable("ContactEmailPermissionEvents");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Source).HasMaxLength(50).IsRequired();
        builder.Property(item => item.EvidenceReference).HasMaxLength(500);
        builder.Property(item => item.ActorId).HasMaxLength(200).IsRequired();
        builder.Property(item => item.OccurredAt).IsRequired();
        builder.HasOne<Contact>().WithMany().HasForeignKey(item => item.ContactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.ContactId, item.OccurredAt });
    }
}
