using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.Accion).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Entidad).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntidadId);
        builder.Property(a => a.Timestamp).IsRequired();
        builder.Property(a => a.DireccionIp).HasMaxLength(45);

        builder.HasIndex(a => new { a.Entidad, a.EntidadId });
        builder.HasIndex(a => a.UserId);
    }
}
