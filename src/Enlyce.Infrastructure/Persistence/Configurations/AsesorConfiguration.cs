using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class AsesorConfiguration : IEntityTypeConfiguration<Asesor>
{
    public void Configure(EntityTypeBuilder<Asesor> builder)
    {
        builder.ToTable("Asesores");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.ComplexProperty(a => a.Correo, eb =>
        {
            eb.Property(e => e.Value)
                .HasColumnName("Correo")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(a => a.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(a => a.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Rol).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Activo).IsRequired();

    }
}
