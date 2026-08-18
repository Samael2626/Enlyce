using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class PoliticaTratamientoConfiguration : IEntityTypeConfiguration<PoliticaTratamiento>
{
    public void Configure(EntityTypeBuilder<PoliticaTratamiento> builder)
    {
        builder.ToTable("PoliticasTratamiento");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Version).HasMaxLength(20).IsRequired();
        builder.Property(p => p.TextoCompleto).HasMaxLength(10000).IsRequired();
        builder.Property(p => p.FechaVigencia).IsRequired();
        builder.Property(p => p.Activa).IsRequired();

        builder.HasIndex(p => p.Version).IsUnique();
    }
}
