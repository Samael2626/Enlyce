using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class ContactImportBatchConfiguration : IEntityTypeConfiguration<ContactImportBatch>
{
    public void Configure(EntityTypeBuilder<ContactImportBatch> builder)
    {
        builder.ToTable("ContactImportBatches");
        builder.HasKey(batch => new { batch.ActorId, batch.RequestId });
        builder.Property(batch => batch.ActorId).HasMaxLength(200);
        builder.Property(batch => batch.RequestId).ValueGeneratedNever();
        builder.Property(batch => batch.PayloadHash).HasMaxLength(64).IsRequired();
        builder.Property(batch => batch.ErrorsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(batch => batch.CreatedAt).IsRequired();
    }
}
