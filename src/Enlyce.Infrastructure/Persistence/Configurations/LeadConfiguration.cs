using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.ComplexProperty(l => l.Email, eb =>
        {
            eb.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.ComplexProperty(l => l.Telefono, tb =>
        {
            tb.Property(t => t.Value)
                .HasColumnName("Telefono")
                .HasMaxLength(20);
        });

        builder.Property(l => l.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Fuente).HasMaxLength(100);
        builder.Property(l => l.Estado).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.MotivoCierre).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.NotasCierre).HasMaxLength(1000);
        builder.Property(l => l.AutorizacionDatos).IsRequired();
        builder.Property(l => l.Activo).IsRequired();

        builder.Property(l => l.TipoOperacion).HasMaxLength(30).IsRequired();
        builder.Property(l => l.OwnerService).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.EtapaPipeline).HasMaxLength(50).IsRequired();
        builder.Property(l => l.InteraccionesCount).IsRequired();
        builder.Property(l => l.FechaUltimaInteraccion);
        builder.Property(l => l.FechaActualizacion).IsRequired();

        builder.HasIndex(l => l.EtapaPipeline);
        builder.HasIndex(l => l.OwnerService);
        builder.HasIndex(l => l.AsesorAsignadoId);
    }
}
