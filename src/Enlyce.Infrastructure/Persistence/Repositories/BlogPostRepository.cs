using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Enlyce.Infrastructure.Persistence.Repositories;

public sealed class BlogPostRepository(EnlyceDbContext context) : IBlogPostRepository
{
    public async Task<IReadOnlyList<BlogPost>> ListAsync(bool includeDrafts, CancellationToken ct = default)
    {
        var query = context.BlogPosts.AsNoTracking().AsQueryable();
        if (!includeDrafts) query = query.Where(post => post.IsPublished);
        return await query.OrderByDescending(post => post.PublishedAtUtc)
            .ThenByDescending(post => post.UpdatedAtUtc).Take(100).ToListAsync(ct);
    }

    public Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.BlogPosts.FirstOrDefaultAsync(post => post.Id == id, ct);

    public Task<BlogPost?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct = default) =>
        context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(
            post => post.Slug == slug && (includeDrafts || post.IsPublished), ct);

    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken ct = default) =>
        context.BlogPosts.AnyAsync(post => post.Slug == slug && post.Id != exceptId, ct);

    public async Task SaveAsync(BlogPost post, CancellationToken ct = default)
    {
        if (context.Entry(post).State == EntityState.Detached) context.BlogPosts.Add(post);
        await context.SaveChangesAsync(ct);
    }
}
