using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class InmuebleConfiguration : IEntityTypeConfiguration<Inmueble>
{
    public void Configure(EntityTypeBuilder<Inmueble> builder)
    {
        builder.ToTable("Inmuebles");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Descripcion).HasMaxLength(2000);
        builder.Property(i => i.InternalFeatures).HasColumnType("text[]").HasDefaultValueSql("ARRAY[]::text[]").IsRequired();
        builder.Property(i => i.ExternalFeatures).HasColumnType("text[]").HasDefaultValueSql("ARRAY[]::text[]").IsRequired();
        builder.Property(i => i.AnioConstruccion);
        builder.Property(i => i.EstadoFisico).HasMaxLength(100);
        builder.Property(i => i.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Modalidad).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Estado).HasConversion<string>().HasMaxLength(20);

        builder.ComplexProperty(i => i.Direccion, db =>
        {
            db.Property(d => d.Calle).HasColumnName("Calle").HasMaxLength(300).IsRequired();
            db.Property(d => d.Ciudad).HasColumnName("Ciudad").HasMaxLength(100).IsRequired();
            db.Property(d => d.Barrio).HasColumnName("Barrio").HasMaxLength(100);
            db.Property(d => d.Pais).HasColumnName("Pais").HasMaxLength(100).HasDefaultValue("Colombia").IsRequired();
            db.Property(d => d.Departamento).HasColumnName("Departamento").HasMaxLength(100);
        });

        builder.ComplexProperty(i => i.Precio, pb =>
        {
            pb.Property(p => p.Monto).HasColumnName("Precio").HasColumnType("decimal(18,2)");
            pb.Property(p => p.Moneda).HasColumnName("Moneda").HasMaxLength(3).IsRequired();
        });

        builder.Property(i => i.PropietarioId).IsRequired();
        builder.Property(i => i.Activo).IsRequired();

        builder.HasIndex(i => i.Tipo);
        builder.HasIndex(i => i.Modalidad);

        // Unico solo entre importados: los creados en el CRM no tienen referencia.
        builder.Property(i => i.ExternalRef).HasMaxLength(100);
        builder.HasIndex(i => i.ExternalRef).IsUnique().HasFilter("\"ExternalRef\" IS NOT NULL");
    }
}
