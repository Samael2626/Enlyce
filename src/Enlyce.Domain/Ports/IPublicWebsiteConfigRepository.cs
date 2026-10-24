using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IPublicWebsiteConfigRepository
{
    Task<PublicWebsiteConfig> GetAsync(CancellationToken ct = default);
    Task UpdateAsync(PublicWebsiteConfig config, CancellationToken ct = default);
}
