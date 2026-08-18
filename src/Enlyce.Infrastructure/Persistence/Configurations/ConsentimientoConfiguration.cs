using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class ConsentimientoConfiguration : IEntityTypeConfiguration<Consentimiento>
{
    public void Configure(EntityTypeBuilder<Consentimiento> builder)
    {
        builder.ToTable("Consentimientos");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.LeadId).IsRequired();
        builder.Property(c => c.Fecha).IsRequired();
        builder.Property(c => c.DireccionIp).HasMaxLength(45);
        builder.Property(c => c.TextoConsentido).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.VersionPolitica).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Metodo).HasMaxLength(50).IsRequired();

        builder.HasOne<Lead>()
            .WithMany()
            .HasForeignKey(c => c.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.LeadId);
    }
}
