using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PublicWebsiteConfigRepository(EnlyceDbContext context) : IPublicWebsiteConfigRepository
{
    public async Task<PublicWebsiteConfig> GetAsync(CancellationToken ct = default)
    {
        var config = await context.PublicWebsiteConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == 1, ct);
        return config ?? PublicWebsiteConfig.CreateDefault();
    }

    public async Task UpdateAsync(PublicWebsiteConfig config, CancellationToken ct = default)
    {
        var existing = await context.PublicWebsiteConfigs.FirstOrDefaultAsync(c => c.Id == 1, ct);
        if (existing is null)
        {
            config.Id = 1;
            config.UpdatedAtUtc = DateTime.UtcNow;
            context.PublicWebsiteConfigs.Add(config);
        }
        else
        {
            existing.AgencyName = config.AgencyName;
            existing.Slogan = config.Slogan;
            existing.HeroTitle = config.HeroTitle;
            existing.HeroSubtitle = config.HeroSubtitle;
            existing.HeroBannerUrl = config.HeroBannerUrl;
            existing.AboutTitle = config.AboutTitle;
            existing.AboutText = config.AboutText;
            existing.ContactPhone = config.ContactPhone;
            existing.ContactWhatsapp = config.ContactWhatsapp;
            existing.ContactEmail = config.ContactEmail;
            existing.Address = config.Address;
            existing.InstagramUrl = config.InstagramUrl;
            existing.FacebookUrl = config.FacebookUrl;
            existing.SeoTitle = config.SeoTitle;
            existing.SeoDescription = config.SeoDescription;
            existing.PrimaryColor = config.PrimaryColor;
            existing.AccentColor = config.AccentColor;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            existing.UpdatedByUserId = config.UpdatedByUserId;
        }

        await context.SaveChangesAsync(ct);
    }
}
