using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public class VisitaConfiguration : IEntityTypeConfiguration<Visita>
{
    public void Configure(EntityTypeBuilder<Visita> builder)
    {
        builder.ToTable("Visitas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Feedback)
            .HasMaxLength(2000);

        builder.Property(v => v.Estado)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne<Lead>()
            .WithMany()
            .HasForeignKey(v => v.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.LeadId);
        builder.HasIndex(v => v.AsesorId);
    }
}
