using Enlyce.Application.Media;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class PropertyPhotoReferenceReadRepository(EnlyceDbContext context) : IPropertyPhotoReferenceReadRepository
{
    public async Task<IReadOnlyList<string>> GetReferencedPhotoUrlsAsync(CancellationToken ct = default) =>
        await context.PropertyPhotos.AsNoTracking().Select(photo => photo.Url).ToListAsync(ct);
}
