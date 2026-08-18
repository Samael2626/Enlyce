using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class InteraccionConfiguration : IEntityTypeConfiguration<Interaccion>
{
    public void Configure(EntityTypeBuilder<Interaccion> builder)
    {
        builder.ToTable("Interacciones");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Tipo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Resumen)
            .HasMaxLength(2000);

        builder.HasOne<Lead>()
            .WithMany()
            .HasForeignKey(i => i.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.LeadId);
    }
}
