using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class PropertyPublicationConfiguration : IEntityTypeConfiguration<PropertyPublication>
{
    public void Configure(EntityTypeBuilder<PropertyPublication> builder)
    {
        builder.ToTable("PropertyPublications");
        builder.HasKey(publication => publication.Id);
        builder.Property(publication => publication.Id).ValueGeneratedNever();

        builder.Property(publication => publication.PropertyId).IsRequired();
        builder.Property(publication => publication.AdvisorId).IsRequired();
        builder.Property(publication => publication.Slug).HasMaxLength(200).IsRequired();
        builder.Property(publication => publication.PublicTitle).HasMaxLength(200).IsRequired();
        builder.Property(publication => publication.PublicDescription).HasMaxLength(4_000).IsRequired();
        builder.Property(publication => publication.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(publication => publication.PublicPriceAmount)
            .HasColumnType("decimal(18,2)");
        builder.Property(publication => publication.PublicPriceCurrency).HasMaxLength(3);
        builder.Property(publication => publication.Municipality).HasMaxLength(100);
        builder.Property(publication => publication.Neighborhood).HasMaxLength(100);
        builder.Property(publication => publication.ApproximateLatitude).HasPrecision(9, 6);
        builder.Property(publication => publication.ApproximateLongitude).HasPrecision(9, 6);
        builder.Property(publication => publication.ExactAddressVisible).IsRequired();
        builder.Property(publication => publication.CreatedAt).IsRequired();

        builder.HasIndex(publication => publication.Slug).IsUnique();
        builder.HasIndex(publication => publication.Status);
        builder.HasIndex(publication => publication.PublishedAt);
        builder.HasIndex(publication => publication.Municipality);
        builder.HasIndex(publication => publication.Neighborhood);
        builder.HasIndex(publication => publication.PublicPriceAmount);

        builder.HasOne<Inmueble>()
            .WithOne()
            .HasForeignKey<PropertyPublication>(publication => publication.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Asesor>()
            .WithMany()
            .HasForeignKey(publication => publication.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(publication => publication.Photos)
            .WithOne()
            .HasForeignKey(photo => photo.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(publication => publication.Photos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
