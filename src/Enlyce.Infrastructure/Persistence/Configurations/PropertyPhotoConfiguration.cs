using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class PropertyPhotoConfiguration : IEntityTypeConfiguration<PropertyPhoto>
{
    public void Configure(EntityTypeBuilder<PropertyPhoto> builder)
    {
        builder.ToTable("PropertyPhotos");
        builder.HasKey(photo => photo.Id);
        builder.Property(photo => photo.Id).ValueGeneratedNever();
        builder.Property(photo => photo.PublicationId).IsRequired();
        builder.Property(photo => photo.Url).HasMaxLength(2_048).IsRequired();
        builder.Property(photo => photo.AltText).HasMaxLength(300).IsRequired();
        builder.Property(photo => photo.Order).IsRequired();
        builder.Property(photo => photo.IsCover).IsRequired();

        builder.HasIndex(photo => new { photo.PublicationId, photo.Order }).IsUnique();
    }
}
