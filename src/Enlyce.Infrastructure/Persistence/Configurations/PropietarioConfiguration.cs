using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class PropietarioConfiguration : IEntityTypeConfiguration<Propietario>
{
    public void Configure(EntityTypeBuilder<Propietario> builder)
    {
        builder.ToTable("Propietarios");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.ComplexProperty(p => p.Email, eb =>
        {
            eb.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.ComplexProperty(p => p.Telefono, tb =>
        {
            tb.Property(t => t.Value)
                .HasColumnName("Telefono")
                .HasMaxLength(20);
        });

        builder.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Activo).IsRequired();
    }
}
