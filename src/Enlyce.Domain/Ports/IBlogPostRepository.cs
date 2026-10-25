using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Ports;

public interface IBlogPostRepository
{
    Task<IReadOnlyList<BlogPost>> ListAsync(bool includeDrafts, CancellationToken ct = default);
    Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BlogPost?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken ct = default);
    Task SaveAsync(BlogPost post, CancellationToken ct = default);
}
