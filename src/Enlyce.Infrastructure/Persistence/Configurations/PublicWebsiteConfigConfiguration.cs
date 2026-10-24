using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class PublicWebsiteConfigConfiguration : IEntityTypeConfiguration<PublicWebsiteConfig>
{
    public void Configure(EntityTypeBuilder<PublicWebsiteConfig> builder)
    {
        builder.ToTable("PublicWebsiteConfig");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.AgencyName).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Slogan).HasMaxLength(200).IsRequired();
        builder.Property(c => c.HeroTitle).HasMaxLength(200).IsRequired();
        builder.Property(c => c.HeroSubtitle).HasMaxLength(500).IsRequired();
        builder.Property(c => c.HeroBannerUrl).HasMaxLength(500);
        builder.Property(c => c.AboutTitle).HasMaxLength(150).IsRequired();
        builder.Property(c => c.AboutText).HasMaxLength(4000).IsRequired();
        builder.Property(c => c.ContactPhone).HasMaxLength(50).IsRequired();
        builder.Property(c => c.ContactWhatsapp).HasMaxLength(50).IsRequired();
        builder.Property(c => c.ContactEmail).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(250).IsRequired();
        builder.Property(c => c.InstagramUrl).HasMaxLength(300);
        builder.Property(c => c.FacebookUrl).HasMaxLength(300);
        builder.Property(c => c.SeoTitle).HasMaxLength(150).IsRequired();
        builder.Property(c => c.SeoDescription).HasMaxLength(500).IsRequired();
        builder.Property(c => c.PrimaryColor).HasMaxLength(20);
        builder.Property(c => c.AccentColor).HasMaxLength(20);
        builder.Property(c => c.UpdatedAtUtc).IsRequired();
    }
}
